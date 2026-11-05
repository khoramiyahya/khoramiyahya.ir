using System.Runtime.ExceptionServices;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.WebAPI.Filters;

internal sealed class DatabaseTransactionFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageTransaction _fileStorageTransaction;

    public DatabaseTransactionFilter(
        ApplicationDbContext dbContext,
        IFileStorageTransaction fileStorageTransaction)
    {
        _dbContext = dbContext;
        _fileStorageTransaction = fileStorageTransaction;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await next();
            return;
        }

        var executionStrategy = new RequestTransactionExecutionStrategy(_dbContext);
        await executionStrategy.ExecuteAsync(() => ExecuteInTransactionAsync(context, next));
    }

    private async Task ExecuteInTransactionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            context.HttpContext.RequestAborted);
        _fileStorageTransaction.Begin();
        var databaseCommitted = false;
        var commitAttempted = false;
        var rollbackAttempted = false;

        try
        {
            var executedContext = await next();
            var statusCode = (executedContext.Result as IStatusCodeActionResult)?.StatusCode
                ?? context.HttpContext.Response.StatusCode;

            if (executedContext.Exception is null && statusCode < StatusCodes.Status400BadRequest)
            {
                await _fileStorageTransaction.PrepareAsync(CancellationToken.None);
                commitAttempted = true;
                await transaction.CommitAsync(CancellationToken.None);
                databaseCommitted = true;
                await _fileStorageTransaction.CompleteAsync(CancellationToken.None);
                return;
            }

            rollbackAttempted = true;
            await RollbackAsync(transaction, preserveFilesOnRollbackFailure: false);
        }
        catch (Exception operationException)
        {
            if (databaseCommitted)
            {
                ExceptionDispatchInfo.Capture(operationException).Throw();
            }

            if (!rollbackAttempted)
            {
                try
                {
                    await RollbackAsync(transaction, preserveFilesOnRollbackFailure: commitAttempted);
                }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "The request failed and one or more transaction resources could not be rolled back.",
                        operationException,
                        rollbackException);
                }
            }

            ExceptionDispatchInfo.Capture(operationException).Throw();
        }
    }

    private async Task RollbackAsync(
        IDbContextTransaction transaction,
        bool preserveFilesOnRollbackFailure)
    {
        Exception? databaseException = null;
        Exception? fileException = null;

        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            databaseException = exception;
        }

        try
        {
            if (databaseException is null || !preserveFilesOnRollbackFailure)
            {
                await _fileStorageTransaction.RollbackAsync(CancellationToken.None);
            }
            else
            {
                await _fileStorageTransaction.AbandonAsync(CancellationToken.None);
            }
        }
        catch (Exception exception)
        {
            fileException = exception;
        }

        if (databaseException is not null && fileException is not null)
        {
            throw new AggregateException(databaseException, fileException);
        }

        if (databaseException is not null || fileException is not null)
        {
            ExceptionDispatchInfo.Capture(databaseException ?? fileException!).Throw();
        }
    }

    private sealed class RequestTransactionExecutionStrategy : ExecutionStrategy
    {
        public RequestTransactionExecutionStrategy(DbContext context)
            : base(context, maxRetryCount: 0, maxRetryDelay: TimeSpan.Zero)
        {
        }

        protected override bool ShouldRetryOn(Exception exception) => false;
    }
}
