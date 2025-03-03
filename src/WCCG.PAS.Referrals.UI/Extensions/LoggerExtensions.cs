using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize referral.")]
    public static partial void FailedToDeserializeReferral(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Referral validation failed: {validationErrors}.")]
    public static partial void ReferralValidationFailed(this ILogger logger, string validationErrors);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving documents from Cosmos DB. Reason {reason}")]
    public static partial void LogErrorRetrievingDocuments(this ILogger logger, Exception exception, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving document from Cosmos DB for id: {documentId}. Reason: {reason}")]
    public static partial void LogErrorRetrievingDocumentById(this ILogger logger, Exception exception, string documentId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error creating or updating document in Cosmos DB. Reason: {reason}")]
    public static partial void LogErrorCreatingOrUpdatingDocument(this ILogger logger, Exception exception, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "New document with id: {documentId} has been added.")]
    public static partial void LogNewDocumentCreated(this ILogger logger, string documentId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Document with id: {documentId} has been updated.")]
    public static partial void LogDocumentUpdated(this ILogger logger, string documentId);
}
