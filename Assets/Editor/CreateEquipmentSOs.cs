// using UnityEditor;
// using UnityEngine;

// /// <summary>
// /// 一键生成 6 件装备的 EquipmentSO，放在 Assets/ScriptableObjects/EquipmentSOs/
// /// 菜单：Tools → Create Equipment SOs
// /// </summary>
// public class CreateEquipmentSOs
// {
//     private const string FolderPath = "Assets/ScriptableObjects/EquipmentSOs";

//     [MenuItem("Tools/Create Equipment SOs")]
//     public static void CreateAll()
//     {
//         EnsureFolderExists();

//         CreateShoulder();
//         CreateTop();
//         CreateBelt();
//         CreateBottom();
//         CreateShoes();
//         CreateWeapon();

//         AssetDatabase.Refresh();

//         Debug.Log("6 件装备 SO 创建完成！请拖入 EquipmentDataBaseSO 中。");
//     }

//     private static void EnsureFolderExists()
//     {
//         if (!AssetDatabase.IsValidFolder(FolderPath))
//         {
//             AssetDatabase.CreateFolder("Assets/ScriptableObjects", "EquipmentSOs");
//         }
//     }

//     private static EquipmentSO Create(string name)
//     {
//         string path = $"{FolderPath}/{name}.asset";
//         if (AssetDatabase.LoadAssetAtPath<EquipmentSO>(path) != null)
//         {
//             AssetDatabase.DeleteAsset(path);
//             AssetDatabase.Refresh();
//         }

//         EquipmentSO so = ScriptableObject.CreateInstance<EquipmentSO>();
//         so.EquipmentEnhanceInfos = new EquipmentEnhanceLevelInfo[4];
//         for (int i = 0; i < 4; i++)
//             so.EquipmentEnhanceInfos[i] = new EquipmentEnhanceLevelInfo();

//         return so;
//     }

//     private static void SaveAsset(EquipmentSO so, string fileName)
//     {
//         string path = $"{FolderPath}/{fileName}.asset";
//         EditorUtility.SetDirty(so);
//         AssetDatabase.CreateAsset(so, path);
//     }

//     // ──────────── 护肩 Shoulder ────────────
//     private static void CreateShoulder()
//     {
//         var so = Create("ShoulderSO");
//         so.EquipmentName = EquipmentConstants.Shoulder;
//         so.Sprite = S("Shoulder");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 5, Attack = 0, Defense = 1 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 10, Attack = 0, Defense = 3, GoldCoinCost = 500, CostItems = { C("PlantSO", 5), C("DarkOreSO", 2) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 20, Attack = 0, Defense = 5, GoldCoinCost = 1000, CostItems = { C("DarkOreSO", 3), C("SilverOreSO", 3) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 35, Attack = 0, Defense = 8, GoldCoinCost = 2000, CostItems = { C("GoldOreSO", 2), C("GemSO", 1) } };
//         SaveAsset(so, "ShoulderSO");
//     }

//     // ──────────── 上衣 Top ────────────
//     private static void CreateTop()
//     {
//         var so = Create("TopSO");
//         so.EquipmentName = EquipmentConstants.Top;
//         so.Sprite = S("Top");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 8, Attack = 0, Defense = 2 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 15, Attack = 0, Defense = 5, GoldCoinCost = 800, CostItems = { C("PlantSO", 8), C("DarkOreSO", 3) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 30, Attack = 0, Defense = 10, GoldCoinCost = 1500, CostItems = { C("DarkOreSO", 5), C("SilverOreSO", 4) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 50, Attack = 0, Defense = 15, GoldCoinCost = 3000, CostItems = { C("GoldOreSO", 3), C("GemSO", 2) } };
//         SaveAsset(so, "TopSO");
//     }

//     // ──────────── 腰带 Belt ────────────
//     private static void CreateBelt()
//     {
//         var so = Create("BeltSO");
//         so.EquipmentName = EquipmentConstants.Belt;
//         so.Sprite = S("Belt");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 10, Attack = 0, Defense = 1 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 20, Attack = 0, Defense = 2, GoldCoinCost = 400, CostItems = { C("PlantSO", 5), C("DarkOreSO", 2) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 40, Attack = 0, Defense = 3, GoldCoinCost = 800, CostItems = { C("DarkOreSO", 3), C("SilverOreSO", 3) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 65, Attack = 0, Defense = 5, GoldCoinCost = 1500, CostItems = { C("GoldOreSO", 2), C("GemSO", 1) } };
//         SaveAsset(so, "BeltSO");
//     }

//     // ──────────── 下装 Bottom ────────────
//     private static void CreateBottom()
//     {
//         var so = Create("BottomSO");
//         so.EquipmentName = EquipmentConstants.Bottom;
//         so.Sprite = S("Bottom");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 6, Attack = 0, Defense = 2 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 12, Attack = 0, Defense = 4, GoldCoinCost = 600, CostItems = { C("PlantSO", 6), C("DarkOreSO", 3) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 25, Attack = 0, Defense = 7, GoldCoinCost = 1200, CostItems = { C("DarkOreSO", 4), C("SilverOreSO", 3) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 40, Attack = 0, Defense = 12, GoldCoinCost = 2500, CostItems = { C("GoldOreSO", 3), C("GemSO", 1) } };
//         SaveAsset(so, "BottomSO");
//     }

//     // ──────────── 鞋子 Shoes ────────────
//     private static void CreateShoes()
//     {
//         var so = Create("ShoesSO");
//         so.EquipmentName = EquipmentConstants.Shoes;
//         so.Sprite = S("Shoes");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 3, Attack = 1, Defense = 0 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 5, Attack = 3, Defense = 1, GoldCoinCost = 300, CostItems = { C("PlantSO", 3), C("DarkOreSO", 1) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 12, Attack = 5, Defense = 2, GoldCoinCost = 600, CostItems = { C("DarkOreSO", 2), C("SilverOreSO", 2) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 20, Attack = 8, Defense = 4, GoldCoinCost = 1200, CostItems = { C("GoldOreSO", 1), C("GemSO", 1) } };
//         SaveAsset(so, "ShoesSO");
//     }

//     // ──────────── 武器 Weapon ────────────
//     private static void CreateWeapon()
//     {
//         var so = Create("WeaponSO");
//         so.EquipmentName = EquipmentConstants.Weapon;
//         so.Sprite = S("Sword");
//         so.EquipmentEnhanceInfos[0] = new EquipmentEnhanceLevelInfo { HP = 0, Attack = 3, Defense = 0 };
//         so.EquipmentEnhanceInfos[1] = new EquipmentEnhanceLevelInfo { HP = 0, Attack = 5, Defense = 0, GoldCoinCost = 1000, CostItems = { C("PlantSO", 10), C("DarkOreSO", 5) } };
//         so.EquipmentEnhanceInfos[2] = new EquipmentEnhanceLevelInfo { HP = 0, Attack = 12, Defense = 0, GoldCoinCost = 2000, CostItems = { C("DarkOreSO", 6), C("SilverOreSO", 5) } };
//         so.EquipmentEnhanceInfos[3] = new EquipmentEnhanceLevelInfo { HP = 0, Attack = 20, Defense = 0, GoldCoinCost = 4000, CostItems = { C("GoldOreSO", 4), C("GemSO", 2) } };
//         SaveAsset(so, "WeaponSO");
//     }
//     // ──────────── 辅助方法 ────────────
//     private static string ItemPath(string name) =>
//         $"Assets/ScriptableObjects/InventoryItemSOs/{name}.asset";

//     private static string SpritePath(string name) =>
//         $"Assets/Images/VillageScene/EnhanceWindow/{name}.asset";

//     private static CostItem C(string itemName, int count)
//     {
//         var item = AssetDatabase.LoadAssetAtPath<InventoryItemSO>(ItemPath(itemName));
//         return new CostItem { InventoryItemSO = item, Count = count };
//     }

//     private static Sprite S(string imageName)
//     {
//         return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(imageName));
//     }
// }
