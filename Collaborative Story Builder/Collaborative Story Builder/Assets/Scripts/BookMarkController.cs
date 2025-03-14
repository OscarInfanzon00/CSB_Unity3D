using UnityEngine;

public class BookMarkController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void saveBookmark(string id){
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");

        string finalBookList;

        if(bookMarkList.Length > 0){
            finalBookList = bookMarkList + "," + id;
        }
        else{
            finalBookList = id;
        }


        PlayerPrefs.SetString("SavedBookMarkList", finalBookList);

    }

    public void getBookmarks(string id){
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");
        string[] result = bookMarkList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        



    }
}
