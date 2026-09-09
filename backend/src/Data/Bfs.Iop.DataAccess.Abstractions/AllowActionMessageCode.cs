namespace Bfs.Iop.DataAccess.Abstractions;

public enum AllowActionMessageCode
{
    Undefined = 0,

    NoValidToken = 1000,
    UserHasNotEnoughRights = 1001,

    ResourceNotFound = 2000,
    ResourceReferenced = 2001,
    ResourceIsPreviousVersion = 2002,
    ResourceIsLocked = 2003,
    ResourceIsPublic = 2004,
    ResourceIsUnlocked = 2005,
    ResourceIsVocabulary = 2006
}
