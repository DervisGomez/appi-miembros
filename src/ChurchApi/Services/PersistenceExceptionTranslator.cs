using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Services;

internal static class PersistenceExceptionTranslator
{
    private const int SqlServerUniqueConstraint = 2627;
    private const int SqlServerDuplicateKey = 2601;
    private const int SqlServerForeignKeyConstraint = 547;

    private const int SqliteConstraint = 19;
    private const int SqliteUniqueConstraint = 2067;
    private const int SqlitePrimaryKeyConstraint = 1555;
    private const int SqliteForeignKeyConstraint = 787;

    public static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return FindProviderException(exception) switch
        {
            SqlException sqlException => sqlException.Number is SqlServerUniqueConstraint or SqlServerDuplicateKey,
            SqliteException sqliteException => sqliteException.SqliteErrorCode == SqliteConstraint
                && (sqliteException.SqliteExtendedErrorCode is SqliteUniqueConstraint or SqlitePrimaryKeyConstraint
                    || sqliteException.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    public static bool IsForeignKeyConstraintViolation(DbUpdateException exception)
    {
        return FindProviderException(exception) switch
        {
            SqlException sqlException => sqlException.Number == SqlServerForeignKeyConstraint,
            SqliteException sqliteException => sqliteException.SqliteErrorCode == SqliteConstraint
                && (sqliteException.SqliteExtendedErrorCode == SqliteForeignKeyConstraint
                    || sqliteException.Message.Contains("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    private static Exception? FindProviderException(Exception exception)
    {
        var current = exception.InnerException;

        while (current is not null)
        {
            if (current is SqlException or SqliteException)
            {
                return current;
            }

            current = current.InnerException;
        }

        return null;
    }
}
