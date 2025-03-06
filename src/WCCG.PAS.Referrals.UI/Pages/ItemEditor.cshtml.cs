using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Extensions;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class ItemEditorModel : BasePageModel
{
    private readonly IReferralService _referralService;
    private readonly IValidator<ReferralDbModel> _validator;
    private readonly ILogger<ItemEditorModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public bool? IsSaved { get; set; }

    [BindProperty]
    public required string? ReferralJson { get; set; }

    public ItemEditorModel(IReferralService referralService, IValidator<ReferralDbModel> validator, ILogger<ItemEditorModel> logger)
    {
        _referralService = referralService;
        _validator = validator;
        _logger = logger;
    }

    public async Task OnGet(string id)
    {
        SetApimSubscriptionKey();
        if (string.IsNullOrWhiteSpace(ApimSubscriptionKey))
        {
            IsSaved = false;
            HandleEmptyApimKey();
            return;
        }

        try
        {
            var referral = await _referralService.GetByIdAsync(ApimSubscriptionKey, id);
            ReferralJson = JsonSerializer.Serialize(referral, _jsonOptions);
        }
        catch (Exception exception)
        {
            HandleErrors(exception.Message);
        }
    }

    public async Task<IActionResult> OnPost()
    {
        SetApimSubscriptionKey();
        if (string.IsNullOrWhiteSpace(ApimSubscriptionKey))
        {
            HandleEmptyApimKey();
            return Page();
        }

        var referral = DeserializeReferral();
        if (referral is null)
        {
            return Page();
        }

        var isValid = await IsReferralValidAsync(referral);
        if (!isValid)
        {
            return Page();
        }

        await UpsertReferralAsync(referral);
        return Page();
    }

    private ReferralDbModel? DeserializeReferral()
    {
        if (ReferralJson is null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<ReferralDbModel>(ReferralJson!);
        }
        catch (JsonException ex)
        {
            _logger.FailedToDeserializeReferral(ex);
            HandleErrors(ex.Message);
        }

        return null;
    }

    private async Task<bool> IsReferralValidAsync(ReferralDbModel referral)
    {
        var validationResult = await _validator.ValidateAsync(referral);

        if (validationResult.IsValid)
        {
            return true;
        }

        var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray();
        _logger.ReferralValidationFailed(string.Join(';', errors));
        HandleErrors(errors);

        return false;
    }

    private async Task UpsertReferralAsync(ReferralDbModel referral)
    {
        try
        {
            await _referralService.UpsertAsync(ApimSubscriptionKey!, referral);
            IsSaved = true;
        }
        catch (Exception ex)
        {
            HandleErrors(ex.Message);
        }
    }

    private void HandleErrors(params string[] errorMessages)
    {
        IsSaved = false;
        ErrorMessage = errorMessages.Aggregate((f, s) => f + "<br/>" + s);
    }
}
