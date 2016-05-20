namespace WebGallery.Services
{
    public interface ISubmitterService
    {
        bool CanModify(string submitterMicrosoftAccount, int appId);
        bool IsSuperSubmitter(string submitterMicrosoftAccount);
        bool HasContactInfo(string submitterMicrosoftAccount);
    }
}
