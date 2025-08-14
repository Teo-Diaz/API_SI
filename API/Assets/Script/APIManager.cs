using System.Collections;
using System; 
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class APIManager : MonoBehaviour
{
    private string fakeApiUrl = "https://my-json-server.typicode.com/KennyM14/API_Activity";

    private string rickAndMortyApiUrl = "https://rickandmortyapi.com/api/character/";

    public TextMeshProUGUI[] cardNameTexts; 
    public TextMeshProUGUI[] cardSpeciesTexts; 
    public Image[] cardImages; 
    public TMP_Dropdown userDropDown; 
    private int currentUserId = -1;

    void Start()
    {
        InitializeDropdown();
        ClearCards(); 
    }

    private void InitializeDropdown()
    {
        userDropDown.ClearOptions();
        List<string> userNames = new List<string> { "Select a User", "Rayne", "Abel", "Lance"};
        userDropDown.AddOptions(userNames);
        userDropDown.value = 0; 
        userDropDown.onValueChanged.AddListener(SwitchUser);
    }   

    private void ClearCards()
    {
        for (int i = 0; i < cardNameTexts.Length; i++)
        {
            cardNameTexts[i].text = "";
            cardSpeciesTexts[i].text = "";
            cardImages[i].sprite = null;
        }
    }

    public void FetchPlayerDeck(int userId)
    {
        StartCoroutine(GetPlayerDeck(userId));
    }

    private IEnumerator GetPlayerDeck(int userId)
    {
        string url = fakeApiUrl + "/users/" + userId;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {

                UserData userData = JsonUtility.FromJson<UserData>(webRequest.downloadHandler.text);
                Debug.Log(userData.name);
                Debug.Log(string.Join(", ", userData.deck));

                ClearCards(); 

                for (int i = 0; i < userData.deck.Length; i++)
                {
                    FetchCharacterInfo(userData.deck[i], i);
                }
            }
        }
    }

    public void FetchCharacterInfo(int characterId, int cardIndex)
    {
        StartCoroutine(GetCharacterInfo(characterId, cardIndex));
    }

    private IEnumerator GetCharacterInfo(int characterId, int cardIndex)
    {
        string url = rickAndMortyApiUrl + characterId;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                CharacterInfo characterInfo = JsonUtility.FromJson<CharacterInfo>(webRequest.downloadHandler.text);
                DisplayCharacterInfo(characterInfo, cardIndex);
            }
        }
    }
    private void DisplayCharacterInfo(CharacterInfo characterInfo, int cardIndex)
    {
        if (cardIndex < cardNameTexts.Length)
        {
            cardNameTexts[cardIndex].text = characterInfo.name;
            cardSpeciesTexts[cardIndex].text = characterInfo.species;

            StartCoroutine(LoadCharacterImage(characterInfo.image, cardIndex));
        }
    }
    private IEnumerator LoadCharacterImage(string imageUrl, int cardIndex)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest(); 

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading image: " + webRequest.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                cardImages[cardIndex].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    public void SwitchUser(int index)
    {
       if (index == 0) 
        {
            ClearCards();
            currentUserId = -1;
            return;
        }
        currentUserId = index; 
        FetchPlayerDeck(currentUserId);
    }

    [Serializable]
    public class UserData
    {
        public int id;
        public string name;
        public int[] deck;
    }

    [Serializable]
    public class CharacterInfo
    {
        public string name;
        public string species;
        public string image;
    }
}