using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerShooting))]
public class WeaponHolderSetup : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PlayerShooting playerShooting = (PlayerShooting)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Настройка точек крепления оружия", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "РУЧНАЯ НАСТРОЙКА ТОЧЕК КРЕПЛЕНИЯ:\n\n" +
            "1. Создайте пустые GameObject'ы как дочерние объекты игрока\n" +
            "2. Назовите их понятно (например: 'VssHolder', 'PistolHolder')\n" +
            "3. Настройте позицию и поворот каждой точки крепления\n" +
            "4. В списке 'Secondary Weapon Holders' добавьте элементы\n" +
            "5. В поле 'Weapon Prefab' перетащите префаб нужного оружия\n" +
            "6. В поле 'Weapon Holder' перетащите созданную точку крепления\n\n" +
            "ПРИМЕР:\n" +
            "Weapon Prefab: (префаб 'Vss Vintores')\n" +
            "Weapon Holder: VssHolder\n\n" +
            "Если точка крепления не настроена для оружия, будет использоваться Secondary Weapon Holder",
            MessageType.Info
        );
        
        if (GUILayout.Button("Показать текущие настройки в консоли"))
        {
            ShowCurrentSettings(playerShooting);
        }
    }
    
    private void ShowCurrentSettings(PlayerShooting playerShooting)
    {
        Debug.Log("=== ТЕКУЩИЕ НАСТРОЙКИ ТОЧЕК КРЕПЛЕНИЯ ===");
        
        if (playerShooting.secondaryWeaponHolder != null)
        {
            Debug.Log($"Fallback точка крепления: {playerShooting.secondaryWeaponHolder.name}");
        }
        else
        {
            Debug.LogWarning("Fallback точка крепления НЕ НАСТРОЕНА!");
        }
        
        if (playerShooting.secondaryWeaponHolders == null || playerShooting.secondaryWeaponHolders.Count == 0)
        {
            Debug.LogWarning("Специальные точки крепления НЕ НАСТРОЕНЫ!");
        }
        else
        {
            Debug.Log($"Настроено {playerShooting.secondaryWeaponHolders.Count} специальных точек крепления:");
            foreach (var mapping in playerShooting.secondaryWeaponHolders)
            {
                if (mapping.weaponPrefab != null && mapping.weaponHolder != null)
                {
                    Debug.Log($"  ✓ {mapping.weaponPrefab.name} -> {mapping.weaponHolder.name}");
                }
                else
                {
                    Debug.LogError($"  ✗ {(mapping.weaponPrefab != null ? mapping.weaponPrefab.name : "ПРЕФАБ НЕ УКАЗАН")} -> {(mapping.weaponHolder != null ? mapping.weaponHolder.name : "ТОЧКА КРЕПЛЕНИЯ НЕ УКАЗАНА")}");
                }
            }
        }
        
        Debug.Log("=== СПИСОК ДОСТУПНЫХ ОРУЖИЙ ===");
        if (playerShooting.weaponPrefabs != null)
        {
            foreach (var weapon in playerShooting.weaponPrefabs)
            {
                if (weapon != null)
                {
                    Debug.Log($"  - {weapon.name.Replace("(Clone)", "").Trim()}");
                }
            }
        }
        
        Debug.Log("=====================================");
    }
}
#endif 