using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bfs.Iop.Core.UnitTests.Data.Exceptions;

[TestFixture(TestOf = typeof(DatabaseExceptionTranslator))]
internal sealed class DatabaseExceptionTranslatorTests
{
    /// <summary>
    /// A PostgresException puts the failing constraint, the column and sometimes the rejected value
    /// into its message. None of that may end up in the translation, because the translation is what
    /// gets serialized into the response.
    /// </summary>
    private const string RevealingMessage =
        "duplicate key value violates unique constraint \"ix_agent_identifier\", Key (identifier)=(CH_BFS) already exists.";

    [TestCase("23505", 409, "DB_UNIQUE_VIOLATION")]
    [TestCase("23503", 409, "DB_FOREIGN_KEY_VIOLATION")]
    [TestCase("23001", 409, "DB_RESTRICT_VIOLATION")]
    [TestCase("23P01", 409, "DB_EXCLUSION_VIOLATION")]
    [TestCase("23502", 400, "DB_NOT_NULL_VIOLATION")]
    [TestCase("23514", 400, "DB_CHECK_VIOLATION")]
    [TestCase("22001", 400, "DB_VALUE_TOO_LONG")]
    [TestCase("22003", 400, "DB_NUMERIC_OUT_OF_RANGE")]
    [TestCase("22P02", 400, "DB_INVALID_VALUE_FORMAT")]
    [TestCase("40001", 409, "DB_SERIALIZATION_FAILURE")]
    [TestCase("40P01", 409, "DB_DEADLOCK_DETECTED")]
    [TestCase("57014", 504, "DB_QUERY_TIMEOUT")]
    [TestCase("57P03", 503, "DB_UNAVAILABLE")]
    [TestCase("53300", 503, "DB_TOO_MANY_CONNECTIONS")]
    [TestCase("42501", 500, "DB_PERMISSION_DENIED")]
    [TestCase("42P01", 500, "DB_SCHEMA_MISMATCH")]
    public void Given_postgres_exception_When_Translate_Then_sql_state_determines_status_and_error_code(
        string sqlState,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        // Arrange
        var exception = CreatePostgresException(sqlState);

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.ErrorCode.Should().Be(expectedErrorCode);
        result.SqlState.Should().Be(sqlState);
        result.InnerException.Should().BeSameAs(exception);
    }

    [TestCase("08000")]
    [TestCase("08006")]
    [TestCase("08P01")]
    public void Given_connection_sql_state_When_Translate_Then_service_is_unavailable(string sqlState)
    {
        // Arrange
        var exception = CreatePostgresException(sqlState);

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(503);
        result.ErrorCode.Should().Be("DB_CONNECTION_FAILURE");
    }

    [Test]
    public void Given_unknown_sql_state_When_Translate_Then_error_code_keeps_the_sql_state()
    {
        // Arrange
        var exception = CreatePostgresException("XX000");

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(500);
        result.ErrorCode.Should().Be("DB_XX000");
        result.SqlState.Should().Be("XX000");
    }

    [TestCase("23505")]
    [TestCase("23502")]
    [TestCase("42P01")]
    [TestCase("XX000")]
    public void Given_revealing_postgres_message_When_Translate_Then_nothing_of_it_is_exposed(string sqlState)
    {
        // Arrange
        var exception = CreatePostgresException(sqlState);

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.Message.Should().NotContain("ix_agent_identifier");
        result.Message.Should().NotContain("CH_BFS");
        result.Message.Should().NotContain("identifier");
        result.Message.Should().NotBe(RevealingMessage);

        // The original stays reachable for the logs.
        result.InnerException!.Message.Should().Contain("ix_agent_identifier");
    }

    [Test]
    public void Given_wrapped_postgres_exception_When_Translate_Then_the_inner_sql_state_is_used()
    {
        // Arrange
        // This is the shape Entity Framework produces while saving.
        var exception = new DbUpdateException("An error occurred while saving.", CreatePostgresException("23503"));

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.ErrorCode.Should().Be("DB_FOREIGN_KEY_VIOLATION");
        result.SqlState.Should().Be("23503");
    }

    [Test]
    public void Given_already_translated_exception_When_Translate_Then_the_translation_is_reused()
    {
        // Arrange
        // The interceptor translates on the command level, then Entity Framework wraps the result.
        var translated = DatabaseExceptionTranslator.Translate(CreatePostgresException("23505"));
        var exception = new DbUpdateException("An error occurred while saving.", translated);

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        result.Should().BeSameAs(translated);
    }

    [Test]
    public void Given_concurrency_exception_When_Translate_Then_it_becomes_a_conflict()
    {
        // Arrange
        var exception = new DbUpdateConcurrencyException("The database operation was expected to affect 1 row(s).");

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(409);
        result.ErrorCode.Should().Be("DB_CONCURRENCY_CONFLICT");
        result.SqlState.Should().BeNull();
    }

    [Test]
    public void Given_npgsql_exception_without_sql_state_When_Translate_Then_service_is_unavailable()
    {
        // Arrange
        var exception = new NpgsqlException("Failed to connect to 10.0.0.1:5432.");

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(503);
        result.ErrorCode.Should().Be("DB_UNAVAILABLE");
        result.Message.Should().NotContain("10.0.0.1");
    }

    [Test]
    public void Given_unrelated_exception_When_Translate_Then_it_becomes_a_generic_server_error()
    {
        // Arrange
        var exception = new DbUpdateException("An error occurred while saving.", new InvalidOperationException("boom"));

        // Act
        var result = DatabaseExceptionTranslator.Translate(exception);

        // Assert
        using var _ = new AssertionScope();
        result.StatusCode.Should().Be(500);
        result.ErrorCode.Should().Be("DB_ERROR");
        result.Message.Should().NotContain("boom");
    }

    [Test]
    public void Given_various_exceptions_When_IsDatabaseFailure_Then_only_database_failures_are_recognized()
    {
        // Assert
        using var _ = new AssertionScope();
        DatabaseExceptionTranslator.IsDatabaseFailure(CreatePostgresException("23505")).Should().BeTrue();
        DatabaseExceptionTranslator.IsDatabaseFailure(new NpgsqlException("boom")).Should().BeTrue();
        DatabaseExceptionTranslator.IsDatabaseFailure(new DbUpdateException("boom")).Should().BeTrue();
        DatabaseExceptionTranslator.IsDatabaseFailure(new OperationCanceledException()).Should().BeFalse();
        DatabaseExceptionTranslator.IsDatabaseFailure(new InvalidOperationException("boom")).Should().BeFalse();
    }

    private static PostgresException CreatePostgresException(string sqlState) =>
        new(RevealingMessage, severity: "ERROR", invariantSeverity: "ERROR", sqlState);
}
