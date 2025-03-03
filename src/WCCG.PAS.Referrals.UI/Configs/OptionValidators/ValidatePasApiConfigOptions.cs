using Microsoft.Extensions.Options;

namespace WCCG.PAS.Referrals.UI.Configs.OptionValidators;

[OptionsValidator]
public partial class ValidateCosmosConfigOptions : IValidateOptions<CosmosConfig>;
