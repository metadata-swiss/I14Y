using System.Collections.ObjectModel;

namespace Bfs.Iop.Core.Vocabularies;

public sealed record ChannelTypesVocabulary : IdentifiedVocabularyBase
{
    public override string Identifier => "EU_Channel_Types";

    // Codes are static and necessary for validation and mapping
    public static class Codes
    {
        public const string PostCode = "0c84394663";

        public const string EmailCode = "1fc1caefa8";

        public const string MobilePhoneCode = "5c12931e3f";

        public const string FaxCode = "a1c5444664";

        public const string WebCode = "b37115f83e";

        public const string PhoneCode = "c05134f579";
    }
}
