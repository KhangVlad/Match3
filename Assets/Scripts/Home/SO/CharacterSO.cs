using System;
using System.Collections.Generic;
using UnityEngine;
using Match3.Enums;


[CreateAssetMenu(fileName = "Game/Character", menuName = "Game/Character")]
public class CharacterSO : ScriptableObject
{
    public CharacterID id;
    public Sprite sprite;
    public string displayName;
    public int age;
}


//public enum CharacterID
//{
//    Alice = 1, // Nàng thị trưởng  
//    Ethan = 2, // Nghiên cứu sinh sinh vật học  
//    Jack = 3, // Anh Thợ Xây  
//    Lily = 4, // Cô gái sinh viên  
//    Emma = 5, // Cô chủ trang trại  
//    Walter = 6, // Cụ Ông Thợ Rèn  
//    Sophia = 7, // Nữ Chủ Tiệm Trà  
//    Noah = 8, // Cháu farm gà  
//    Leo = 9, // Chàng trai chăn bò  
//    Mia = 10, // Cô bé mọt sách nhưng tinh nghịch  
//    Shiba = 11, // Shiba của nàng thị trưởng  
//    Bruno = 12, // Cậu Vàng  
//    Luna = 13, // Bé mèo hoang  
//    Oliver = 14, // Cậu bé tinh nghịch  
//    Thomas = 15, // Thầy giáo tận tuỵ  
//    Henry = 16, // Ông bác trang trại  
//    Felix = 17, // Cậu bé côn trùng  
//    GrannyMay = 18, // Cụ Bà farm gà  
//    Clara = 19, // Bà chủ cửa hàng tiện lợi  
//    Max = 20 // Anh tài xế xe bus  
//}