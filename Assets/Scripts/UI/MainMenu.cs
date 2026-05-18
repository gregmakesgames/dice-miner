using System.Linq;
using Cysharp.Threading.Tasks;
using DiceMiner.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DiceMiner.UI
{
    public class MainMenu : MonoBehaviour
    {
        public Button loadGameActiveButton;
        public Button loadGameDisabledButton;
        
        private void Start()
        {
            var haveSave = Game.save.GetSaves().Count > 0;
            
            loadGameActiveButton.gameObject.SetActive(haveSave);
            loadGameDisabledButton.gameObject.SetActive(!haveSave);
        }
        
        public async void OnNewGameClicked()
        {
            var savedGame = Game.save.CreateNewGame();
            await StartWithSave(savedGame);
        }
        
        public async void OnLoadGameClicked()
        {
            var savedGame = Game.save.GetSaves().First();
            await StartWithSave(savedGame);
        }

        private async UniTask StartWithSave(SavedGame savedGame)
        {
            await CrossFadeController.StartCrossFade();
            
            Game.runStarter.StartRun(savedGame);
            
            await CrossFadeController.EndCrossFade();
        }
    }
}