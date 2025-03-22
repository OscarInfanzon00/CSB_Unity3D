using UnityEngine;

public class BookMarkController : MonoBehaviour
{

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

    public void getBookmarks(){
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");
        string[] result = bookMarkList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        



    }
}
