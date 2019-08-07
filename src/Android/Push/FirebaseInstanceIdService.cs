#if !FDROID
using Android.App;
using Android.Content;
using Bit.App.Abstractions;
using Bit.Core;
using Bit.Core.Abstractions;
using Bit.Core.Utilities;
using Firebase.Iid;

namespace Bit.Droid.Push
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseInstanceIdService : Firebase.Iid.FirebaseInstanceIdService
    {
        public async override void OnTokenRefresh()
        {
            var storageService = ServiceContainer.Resolve<IStorageService>("storageService");
            var pushNotificationService = ServiceContainer.Resolve<IPushNotificationService>("pushNotificationService");
            await storageService.SaveAsync(Constants.PushRegisteredTokenKey, FirebaseInstanceId.Instance.Token);
            await pushNotificationService.RegisterAsync();
        }
    }
}
#endif
