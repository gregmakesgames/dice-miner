using Cysharp.Threading.Tasks;
using DiceMiner.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceMiner.UI
{
    public class MainMenu : MonoBehaviour
    {
        public async void OnNewGameClicked()
        {
            var savedGame = SavedGame.New();
            GameSaver.SaveToPlayerPrefs(savedGame);
            await StartWithSave(savedGame);
        }
        
        public async void OnLoadGameClicked()
        {
            var savedGame = GameSaver.LoadFromPlayerPrefs();
            await StartWithSave(savedGame);
        }

        private async UniTask StartWithSave(SavedGame savedGame)
        {
            await CrossFadeController.StartCrossFade();
            await SceneManager.LoadSceneAsync("GameplayScene");
            var gameplayController = FindFirstObjectByType<GameplayController>();
            await gameplayController.PrepareSave(savedGame);
            await CrossFadeController.EndCrossFade();
        }
    }
}