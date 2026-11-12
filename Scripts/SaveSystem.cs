//セーブデータをファイルに書き込む
//セーブデータをファイルから読み込む

using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    //セーブスロットの設定
    public const int MinSlot = 1;
    public const int MaxSlot = 3;

    //セーブデータの保存先を取得
    private static string GetSavePath(int slot)
    {
        if(slot < MinSlot || slot > MaxSlot)
        {
            Debug.LogError($"無効なセーブスロットです:{slot}");
            return null;
        }

        return Path.Combine(
            Application.persistentDataPath,
            "save" + slot + ".json"
            );
    }

    //セーブデータを保存
    public static bool Save(int slot, SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("保存するデータがありません");
            return false;
        }

        string path = GetSavePath(slot);

        if(string.IsNullOrEmpty(path)) return false;

        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        try
        {
            //保存先フォルダを作成
            Directory.CreateDirectory(Application.persistentDataPath);

            //JSONに変換
            string json = JsonUtility.ToJson(data, true);

            //一時ファイルに書き込む
            File.WriteAllText(tempPath, json);

            //既存のセーブデータがある場合
            if(File.Exists(path))
            {
                //古いバックアップがあれば削除
                if(File.Exists(backupPath)) File.Delete(backupPath);

                //既存データをバックアップ
                File.Replace(tempPath, path, backupPath);

                //新しい保存が成功したらバックアップを削除
                try
                {
                    if(File.Exists(backupPath)) File.Delete(backupPath);
                }
                catch(Exception e)
                {
                    Debug.LogWarning($"バックアップの削除に失敗しました: {e.Message}");
                }
            }
            else
            {
                //初回保存の場合
                File.Move(tempPath, path);
            }

            Debug.Log($"セーブ完了:スロット{slot}");
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError($"セーブに失敗しました:{e.Message}\n保存先:{path}");

            //バックアップが残っていて、現在のセーブデータがない場合は復元を試みる
            try
            {
                if(!File.Exists(path) && File.Exists(backupPath))
                {
                    File.Move(backupPath, path);
                    Debug.LogWarning("バックアップからセーブデータを復元しました");
                }
            }
            catch(Exception restoreException)
            {
                Debug.LogError("バックアップの復元に失敗しました:" + restoreException.Message);
            }

            return false;
        }
        finally
        {
            //残った一時ファイルを削除
            try
            {
                if(File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch(Exception e)
            {
                Debug.LogWarning($"一時ファイルの削除に失敗しました: {e.Message}");
            }
        } 
    }

    //セーブデータを読み込む
    public static SaveData Load(int slot)
    {
        string path = GetSavePath(slot);

        if(string.IsNullOrEmpty(path)) return null;

        //ファイルが存在しない場合は空のデータを返す
        if(!File.Exists(path)) return new SaveData();

        try
        {
            string json = File.ReadAllText(path);

            if(string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"セーブデータが空です:スロット{slot}");
                return null;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if(data == null)
            {
                Debug.LogWarning($"セーブデータを読み込めません:スロット{slot}");
                return null;
            }

            //配列がnullの場合は空の配列にする
            if(data.friendPenguinIds == null) data.friendPenguinIds = new int[0];
            if(data.penguins == null) data.penguins = new PenguinSaveData[0];
            if(data.usedOrbIds == null) data.usedOrbIds = new string[0];

            return data;
        }
        catch(Exception e)
        {
            Debug.LogError($"ロードに失敗しました:{e.Message}");
            return null;
        }  
    }

    //セーブデータが存在するか確認
    public static bool Exists(int slot)
    {
        string path = GetSavePath(slot);

        if(string.IsNullOrEmpty(path)) return false;

        try
        {
            return File.Exists(path);
        }
        catch(Exception e)
        {
            Debug.LogError($"セーブデータの確認に失敗しました:{e.Message}");
            return false;
        }
    }

    //セーブデータ削除
    public static bool Delete(int slot)
    {
        string path = GetSavePath(slot);

        if(string.IsNullOrEmpty(path)) return false;

        try
        {
            if(!File.Exists(path)){
                Debug.LogWarning($"削除するデータがありません:スロット{slot}");
                return false;
            }

            File.Delete(path);

            Debug.Log($"セーブデータを削除しました:スロット{slot}");
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError($"セーブデータの削除に失敗しました:{e.Message}");
            return false;
        }
    }

}