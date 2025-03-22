using UnityEngine;

public class BookMarkController : MonoBehaviour
{
    public void saveBookmark(string id)
    {
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");
        Debug.Log($"Current Bookmarks: {bookMarkList}");

        string finalBookList;

        if (bookMarkList.Length > 0)
        {
            finalBookList = bookMarkList + "," + id;
        }
        else
        {
            finalBookList = id;
        }

        PlayerPrefs.SetString("SavedBookMarkList", finalBookList);
        PlayerPrefs.Save();

        Debug.Log($"Updated Bookmarks: {finalBookList}");
    }
}
