using UnityEngine;

namespace MGDK.GameFlow
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject gameInitiatorPrefab;// Drag your prefab here in the Inspector

        public void PlayGame()
        {
            PlayGameAsync();
        }

        private async void PlayGameAsync()
        {
            // Instantiate the GameObject and attach the GameInitiator script
            GameObject gameObject = Instantiate(gameInitiatorPrefab);

            // Get the GameInitiator component (or its derived type)
            GameInitiator gameInitiator = gameObject.GetComponent<GameInitiator>();

            // Call the StartGameAsync method
            if (gameInitiator != null)
            {
                await gameInitiator.StartGameAsync();
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}