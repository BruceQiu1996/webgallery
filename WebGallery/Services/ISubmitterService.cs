namespace WebGallery.Services
{
    public interface ISubmitterService
    {
        bool CanModify(string submitterMicrosoftAccount, int submissionId);
        bool IsSuperSubmitter(string submitterMicrosoftAccount);
        bool HasContactInfo(string submitterMicrosoftAccount);
    }
}
