using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize referral.")]
    public static partial void FailedToDeserializeReferral(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to upsert referral.")]
    public static partial void FailedToUpsertReferral(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Referral validation failed: {validationErrors}.")]
    public static partial void ReferralValidationFailed(this ILogger logger, string validationErrors);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving documents from Cosmos DB.")]
    public static partial void LogErrorRetrievingDocuments(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving document from Cosmos DB for id: {documentId}.")]
    public static partial void LogErrorRetrievingDocumentById(this ILogger logger, Exception exception, string documentId);
}
