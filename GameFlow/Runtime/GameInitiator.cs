using System.Threading.Tasks;  // Needed for Task
using UnityEngine;

namespace MGDK.GameFlow
{
    public abstract class GameInitiator : MonoBehaviour
    {
        // Abstract methods that need to be implemented in derived classes
        public abstract Task BindObjectsAsync();// Instantiate any referneces and set to references. - Connect instances
        public abstract Task InitializeObjectsAsync();// Like 3rd party services
        public abstract Task CreationAsync();// Load Heavy Objects into scene async - resources - asset bundles - addressables. Multiple enemies.
        public abstract Task PreparationAsync();// Setting up our objects. Loading data, positioning, changing apprearence.
        public abstract Task BeginGameAsync();

        public async Task StartGameAsync()
        {
            await BindObjectsAsync();
            await InitializeObjectsAsync();
            await CreationAsync();
            await PreparationAsync();
            await BeginGameAsync();
        }
    }
}