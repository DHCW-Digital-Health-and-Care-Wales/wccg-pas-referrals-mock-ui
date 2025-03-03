using Microsoft.Extensions.Options;

namespace WCCG.PAS.Referrals.UI.Configuration.OptionValidators;

[OptionsValidator]
public partial class ValidateCosmosConfigOptions : IValidateOptions<CosmosConfig>;
