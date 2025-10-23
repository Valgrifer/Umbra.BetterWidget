using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup
{
    /// <summary>
    /// Gets all profiles via reflection and returns ProfileWrapper instances.
    /// </summary>
    /// <returns>IEnumerable of ProfileWrapper, or empty if failed.</returns>
    private IEnumerable<ProfileWrapper> GetProfiles()
    {
        const string mgrTypeName = "Dalamud.Plugin.Internal.Profiles.ProfileManager";
        const string serviceTypeName = "Dalamud.Service`1";
        const string memberName = "Profiles";

        // Find the assembly that contains ProfileManager
        Assembly asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetType(mgrTypeName, false) != null)
            ?? throw new InvalidOperationException($"Assembly containing type {mgrTypeName} not found.");

        Type mgrType = asm.GetType(mgrTypeName, throwOnError: false)
            ?? throw new InvalidOperationException($"Type {mgrTypeName} not found.");

        // Find the generic class Service<ProfileManager>
        Type serviceGeneric = asm.GetType(serviceTypeName, throwOnError: false)!;
        if (serviceGeneric == null || !serviceGeneric.IsGenericTypeDefinition)
            throw new InvalidOperationException($"Generic type {serviceTypeName} not found.");

        // Build Service<ProfileManager>
        Type serviceOfMgr = serviceGeneric.MakeGenericType(mgrType);

        // Find the static Get method from Service<ProfileManager>
        MethodInfo? getMethod = serviceOfMgr.GetMethod("Get", BindingFlags.Public | BindingFlags.Static);
        if (getMethod == null)
            throw new InvalidOperationException($"Static method Get not found on {serviceOfMgr.FullName}.");

        // Call Get() to obtain the ProfileManager instance
        object mgrInstance = getMethod.Invoke(null, null)
            ?? throw new InvalidOperationException($"Call to Get() returned null for {mgrType.FullName}.");

        // Access the Profiles collection (now as an instance member)
        FieldInfo? field = mgrType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object? rawProfiles = null;
        if (field != null)
        {
            rawProfiles = field.GetValue(mgrInstance);
        }
        else
        {
            PropertyInfo? prop = mgrType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
                rawProfiles = prop.GetValue(mgrInstance, null);
        }

        if (rawProfiles == null)
            throw new InvalidOperationException($"Member {memberName} not found or null on instance of {mgrType.FullName}.");

        if (rawProfiles is IEnumerable rawEnum)
        {
            List<ProfileWrapper> profiles = [];
            
            foreach (object item in rawEnum)
            {
                if (item == null) continue;
                ProfileWrapper wrapper = new (item);
                if (wrapper.IsDefaultProfile) continue;
                profiles.Add(wrapper);
            }
            
            return profiles;
        }
        throw new InvalidOperationException($"Member {memberName} on {mgrType.FullName} does not implement IEnumerable.");
    }


    /// <summary>
    /// Wrapper around a Dalamud "Profile" object, exposing only the required members.
    /// </summary>
    public class ProfileWrapper
    {
        private readonly object _inner;
        private readonly PropertyInfo _propIsDefaultProfile;
        private readonly PropertyInfo _propIsEnabled;
        private readonly PropertyInfo _propGuid;
        private readonly PropertyInfo _propName;
        private readonly MethodInfo _methodSetStateAsync;

        internal ProfileWrapper(object inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            var innerType = inner.GetType();

            _propIsDefaultProfile = innerType.GetProperty("IsDefaultProfile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Property ‘IsDefaultProfile’ not found on type {innerType.FullName}.");
            _propIsEnabled = innerType.GetProperty("IsEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Property ‘IsEnabled’ not found on type {innerType.FullName}.");
            _propGuid = innerType.GetProperty("Guid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Property ‘Guid’ not found on type {innerType.FullName}.");
            _propName = innerType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Property ‘Name’ not found on type {innerType.FullName}.");
            _methodSetStateAsync = innerType.GetMethod("SetStateAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Method ‘SetStateAsync’ not found on type {innerType.FullName}.");
        }

        /// <summary>
        /// Indicates if this is the default profile.
        /// </summary>
        public bool IsDefaultProfile => (bool)_propIsDefaultProfile.GetValue(_inner)!;

        /// <summary>
        /// Indicates whether the profile is enabled.
        /// </summary>
        public bool IsEnabled => (bool)_propIsEnabled.GetValue(_inner)!;

        /// <summary>
        /// The profile’s unique identifier (GUID).
        /// </summary>
        public Guid Guid => (Guid)_propGuid.GetValue(_inner)!;

        /// <summary>
        /// The profile's name.
        /// </summary>
        public string Name => (string)_propName.GetValue(_inner)!;

        /// <summary>
        /// Changes the profile’s state (via the internal SetStateAsync method).
        /// </summary>
        /// <param name="enabled">The new state (enabled/disabled).</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task SetStateAsync(bool enabled)
        {
            var result = _methodSetStateAsync.Invoke(_inner, [enabled, true]);
            if (result is Task t)
                return t;
            throw new InvalidOperationException($"Invoke of SetStateAsync did not return a Task on type {_inner.GetType().FullName}.");
        }
    }
}