using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    public List<WaveConfig> AllWavesConfiguration;
    public List<GameObject> AllEnemiesConfiguration;
    public ShipRank PlayerRank;

    public List<WaveConfig> WaveConfigs;
    int startWave;
    int enemiesAmount = 84;

    [HideInInspector]
    public bool SpawnDone;

    GameProgressionHUD _gameProgression;

    void Start () {
        WaveConfigs = GetWaves ();

        startWave = 0;
        StartCoroutine (SpawnAllWaves ());

        _gameProgression = GameObject.Find ("UI").GetComponent<GameProgressionHUD> ();
    }

    void OnEnable () {
        if (WaveConfigs.Count > 0)
            StartCoroutine (SpawnAllWaves ());
    }

    List<WaveConfig> GetWaves () {
        //waves
        var basicWaves = AllWavesConfiguration.GetRange (0, 8);
        var repeatableWaves = AllWavesConfiguration.GetRange (8, 8);
        var bossWaves = AllWavesConfiguration.GetRange (16, 4);

        var waves = new List<WaveConfig> ();

        //FLAGS
        var insertIndexes = new List<int> ();

        var repFlag = 0;
        var insertRepIndexes = new List<int> ();

        var bossFlag = 0;
        var currentBossIndex = 0;

        //ALGORITHM
        for (int i = 1; i <= enemiesAmount; i++) {
            repFlag++;
            bossFlag++;

            //BOSS
            if (bossFlag == 21) {
                waves.Add (bossWaves[currentBossIndex]);
                currentBossIndex++;

                repFlag = 0;
                bossFlag = 0;

                continue;
            }

            //REPEATABLE
            if (repFlag == 5) {
                var repRandomIndex = -1;

                do
                    repRandomIndex = Random.Range (0, repeatableWaves.Count);
                while (insertRepIndexes.Any (_ => _ == repRandomIndex));

                waves.Add (repeatableWaves[repRandomIndex]);
                insertRepIndexes.Add (repRandomIndex);

                repFlag = 0;

                if (insertRepIndexes.Count == repeatableWaves.Count) insertRepIndexes.Clear ();

                continue;
            }

            //NORMAL
            var randomIndex = -1;

            do
                randomIndex = Random.Range (0, basicWaves.Count);
            while (insertIndexes.Any (_ => _ == randomIndex));

            waves.Add (basicWaves[randomIndex]);
            insertIndexes.Add (randomIndex);

            if (insertIndexes.Count == basicWaves.Count) insertIndexes.Clear ();
        }

        return waves;
    }

    IEnumerator SpawnAllWaves () {
        var enemieEnergy = EnemieEnergy ();
        yield return new WaitForSeconds (0.01f);

        for (int i = startWave; i < WaveConfigs.Count; i++) {
            /*
                Normal/Splitter/Strong Ranks - Weak = 1 ; Normal = 2 3 ; Strong = 4
                X Ranks - Weak = 1 2 3 ; Normal = 4 5 6 7 ; Strong = 8 9 10
             */
            var playerIsX = PlayerRank.currentRank.ShipType == ShipType.X;
            var enemyLevel = System.Convert.ToInt32 (PlayerRank.currentRank.Level / (playerIsX ? 4 : 2));
            var playerDifficultyRatio = (PlayerRank.GetCurrentLevel () / 6f) + 1;

            var wave = WaveConfigs[i];

            /*test area */
            var validation = false;
            //validation = wave.IsBoss; //JUST SHIPS
            //validation = !wave.IsBoss; //JUST BOSS
            //validation = !wave.IsBoss && !wave.RepeatPathing; //BOSS/REPEATABLES
            //  validation = !wave.EnemyPrefab.name.Contains ("Diablo"); //SPECIFIC BOSS

            if (validation) {
                startWave++;
                continue;
            }

            /*test area */

            var energy = 0;

            if (wave.IsBoss) {
                wave.TimeBetweenWaves = 600;
                wave.SpawnRandomFactor = 0;
                wave.NumberOfEnemies = 1;
                wave.MoveSpeed = 4;
                //energy = 10;
                energy = enemieEnergy.Where (_ =>
                    wave.EnemyPrefab.name.Contains (_.EnemyType) &&
                    _.ShipType == PlayerRank.currentRank.ShipType &&
                    _.Level == PlayerRank.currentRank.Level).FirstOrDefault ().Energy;
            } else if (wave.RepeatPathing) {
                wave.TimeBetweenWaves = 10;
                wave.SpawnRandomFactor = 1;
                wave.NumberOfEnemies = 1 * (int) playerDifficultyRatio;
                wave.MoveSpeed = Random.Range (5f, 5f) * (playerDifficultyRatio / 2);
                var pathDirection = new Vector3 (0f, Random.Range (0f, 1f) >= 0.5f ? 0 : 180, 0f);
                wave.PathPrefab.transform.Rotate (pathDirection);
                var enemyIndex = Random.Range (0, 5);
                //var enemyIndex = 3; //specific ship
                wave.EnemyPrefab = AllEnemiesConfiguration[enemyIndex + (5 * enemyLevel)];

                energy = enemieEnergy.Where (_ =>
                    wave.EnemyPrefab.name.Contains (_.EnemyType) &&
                    _.ShipType == PlayerRank.currentRank.ShipType &&
                    _.Level == PlayerRank.currentRank.Level).FirstOrDefault ().Energy * 2;
            } else {
                wave.TimeBetweenWaves = Random.Range (3, 8);
                wave.SpawnRandomFactor = Random.Range (0.1f, 1.5f);
                wave.NumberOfEnemies = Random.Range (3, 8);
                wave.MoveSpeed = Random.Range (3f, 5f) * (playerDifficultyRatio / 2);
                var pathDirection = new Vector3 (0f, Random.Range (0f, 1f) >= 0.5f ? 0 : 180, 0f);
                wave.PathPrefab.transform.Rotate (pathDirection);
                var enemyIndex = Random.Range (0, 3);
                //var enemyIndex = 4; //specific ship
                wave.EnemyPrefab = AllEnemiesConfiguration[enemyIndex + (5 * enemyLevel)];

                energy = enemieEnergy.Where (_ =>
                    wave.EnemyPrefab.name.Contains (_.EnemyType) &&
                    _.ShipType == PlayerRank.currentRank.ShipType &&
                    _.Level == PlayerRank.currentRank.Level).FirstOrDefault ().Energy;
            }

            startWave++;
            _gameProgression.UpdateProgressionBar (i + 1, enemiesAmount);

            yield return StartCoroutine (SpawnWave (wave, energy));
        }

        yield return new WaitUntil (() => {
            var enemies = GameObject.FindGameObjectsWithTag ("Enemy");
            var bosses = GameObject.FindGameObjectsWithTag ("Boss");

            return enemies.Length == 0 && bosses.Length == 0;
        });

        //GameObject.Find ("StageController").GetComponent<StageController> ().Fanfare ();
    }

    IEnumerator SpawnWave (WaveConfig WaveConfig, int energy) {
        GameObject enemy = null;

        if (WaveConfig.IsBoss) {
            yield return new WaitUntil (() => GameObject.FindGameObjectsWithTag ("Enemy").Count () == 0);
        }

        for (int i = 0; i < WaveConfig.NumberOfEnemies; i++) {
            enemy = Instantiate (
                WaveConfig.EnemyPrefab,
                WaveConfig.Waypoints () [0].position,
                Quaternion.identity);
            enemy.GetComponent<EnemyPathing> ().SetWaveConfig (WaveConfig);
            enemy.GetComponent<Enemy> ().Health = energy;

            yield return new WaitForSeconds (WaveConfig.SpawnRandomFactor);
        }
        if (WaveConfig.RepeatPathing || WaveConfig.IsBoss) {
            if (WaveConfig.IsBoss) {
                GameObject.Find ("StageController").GetComponent<StageController> ().BossEncounter ();
            }

            var timer = 0f;
            yield return new WaitUntil (() => {
                timer += Time.deltaTime;
                return enemy == null || timer >= WaveConfig.TimeBetweenWaves;
            });
        } else {
            yield return new WaitForSeconds (WaveConfig.TimeBetweenWaves);
        }
        //yield return new WaitForSeconds (0f);
    }

    IEnumerable<EnemyLevel> EnemieEnergy () {
        var enemyLevels = new List<EnemyLevel> ();
        enemyLevels.AddRange (GetBoomerLevels ());
        enemyLevels.AddRange (GetDodgerLevels ());
        enemyLevels.AddRange (GetGunnerLevels ());
        enemyLevels.AddRange (GetJetLevels ());
        enemyLevels.AddRange (GetWarriorLevels ());
        enemyLevels.AddRange (GetBossesLevels ());
        return enemyLevels;
    }

    IEnumerable<EnemyLevel> GetBoomerLevels () {
        var boomerLevels = new List<EnemyLevel> ();
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Normal, 1, 30));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Normal, 2, 60));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Normal, 3, 100));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Normal, 4, 140));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Splitter, 1, 25));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Splitter, 2, 25));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Splitter, 3, 30));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Splitter, 4, 40));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Blaster, 1, 40));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Blaster, 2, 60));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Blaster, 3, 100));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.Blaster, 4, 140));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 1, 40));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 2, 50));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 3, 60));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 4, 70));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 5, 80));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 6, 90));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 7, 120));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 8, 140));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 9, 160));
        boomerLevels.Add (new EnemyLevel ("Boomer", ShipType.X, 10, 180));
        return boomerLevels;
    }
    IEnumerable<EnemyLevel> GetDodgerLevels () {
        var dodgerLevels = new List<EnemyLevel> ();
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Normal, 1, 20));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Normal, 2, 30));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Normal, 3, 50));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Normal, 4, 60));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Splitter, 1, 10));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Splitter, 2, 10));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Splitter, 3, 20));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Splitter, 4, 25));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Blaster, 1, 20));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Blaster, 2, 30));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Blaster, 3, 50));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.Blaster, 4, 60));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 1, 20));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 2, 20));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 3, 30));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 4, 30));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 5, 40));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 6, 40));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 7, 50));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 8, 50));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 9, 60));
        dodgerLevels.Add (new EnemyLevel ("Dodger", ShipType.X, 10, 60));
        return dodgerLevels;
    }
    IEnumerable<EnemyLevel> GetGunnerLevels () {
        var gunnerLevels = new List<EnemyLevel> ();
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Normal, 1, 20));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Normal, 2, 40));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Normal, 3, 50));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Normal, 4, 70));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Splitter, 1, 20));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Splitter, 2, 20));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Splitter, 3, 20));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Splitter, 4, 30));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Blaster, 1, 20));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Blaster, 2, 40));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Blaster, 3, 60));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.Blaster, 4, 70));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 1, 30));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 2, 30));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 3, 40));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 4, 40));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 5, 50));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 6, 50));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 7, 70));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 8, 80));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 9, 90));
        gunnerLevels.Add (new EnemyLevel ("Gunner", ShipType.X, 10, 100));
        return gunnerLevels;
    }
    IEnumerable<EnemyLevel> GetJetLevels () {
        var jetLevels = new List<EnemyLevel> ();
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Normal, 1, 10));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Normal, 2, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Normal, 3, 30));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Normal, 4, 40));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Splitter, 1, 10));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Splitter, 2, 5));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Splitter, 3, 10));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Splitter, 4, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Blaster, 1, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Blaster, 2, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Blaster, 3, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.Blaster, 4, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 1, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 2, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 3, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 4, 20));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 5, 30));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 6, 30));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 7, 30));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 8, 40));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 9, 40));
        jetLevels.Add (new EnemyLevel ("Jet", ShipType.X, 10, 40));
        return jetLevels;
    }
    IEnumerable<EnemyLevel> GetWarriorLevels () {
        var warriorLevels = new List<EnemyLevel> ();
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Normal, 1, 30));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Normal, 2, 40));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Normal, 3, 60));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Normal, 4, 80));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Splitter, 1, 20));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Splitter, 2, 20));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Splitter, 3, 25));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Splitter, 4, 35));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Blaster, 1, 20));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Blaster, 2, 40));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Blaster, 3, 70));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.Blaster, 4, 90));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 1, 30));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 2, 40));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 3, 50));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 4, 60));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 5, 70));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 6, 80));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 7, 90));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 8, 100));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 9, 110));
        warriorLevels.Add (new EnemyLevel ("Warrior", ShipType.X, 10, 120));
        return warriorLevels;
    }
    IEnumerable<EnemyLevel> GetBossesLevels () {
        var bossLevels = new List<EnemyLevel> ();
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Normal, 1, 800));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Normal, 2, 1800));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Normal, 3, 3500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Normal, 4, 5000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Splitter, 1, 600));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Splitter, 2, 700));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Splitter, 3, 1200));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Splitter, 4, 1800));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Blaster, 1, 800));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Blaster, 2, 1500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Blaster, 3, 3000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.Blaster, 4, 4000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 1, 2500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 2, 3000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 3, 3500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 4, 4000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 5, 5500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 6, 6500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 7, 7500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 8, 9000));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 9, 10500));
        bossLevels.Add (new EnemyLevel ("R.E.X", ShipType.X, 10, 12000));

        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Normal, 1, 400));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Normal, 2, 900));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Normal, 3, 1750));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Normal, 4, 2500));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Splitter, 1, 300));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Splitter, 2, 350));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Splitter, 3, 600));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Splitter, 4, 900));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Blaster, 1, 400));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Blaster, 2, 750));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Blaster, 3, 1500));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.Blaster, 4, 2000));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 1, 1000));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 2, 1500));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 3, 1750));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 4, 2000));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 5, 2500));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 6, 3000));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 7, 3750));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 8, 4500));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 9, 5000));
        bossLevels.Add (new EnemyLevel ("Mummy", ShipType.X, 10, 6000));

        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Normal, 1, 1200));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Normal, 2, 2700));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Normal, 3, 5250));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Normal, 4, 7500));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Splitter, 1, 900));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Splitter, 2, 1050));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Splitter, 3, 1800));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Splitter, 4, 2700));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Blaster, 1, 1200));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Blaster, 2, 2250));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Blaster, 3, 4500));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.Blaster, 4, 6000));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 1, 3750));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 2, 4500));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 3, 5250));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 4, 6000));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 5, 8250));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 6, 9750));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 7, 11250));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 8, 13500));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 9, 15750));
        bossLevels.Add (new EnemyLevel ("Alastor", ShipType.X, 10, 18000));

        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Normal, 1, 3000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Normal, 2, 5000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Normal, 3, 10000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Normal, 4, 20000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Splitter, 1, 2000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Splitter, 2, 5000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Splitter, 3, 7000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Splitter, 4, 10000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Blaster, 1, 3000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Blaster, 2, 5000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Blaster, 3, 10000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.Blaster, 4, 20000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 1, 5000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 2, 6000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 3, 7000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 4, 8000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 5, 11000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 6, 15000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 7, 17000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 8, 20000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 9, 25000));
        bossLevels.Add (new EnemyLevel ("Diablo", ShipType.X, 10, 30000));
        return bossLevels;
    }

    private class EnemyLevel {
        public string EnemyType;
        public ShipType ShipType;
        public int Level;
        public int Energy;

        public EnemyLevel (string enemyType, ShipType shipType, int level, int energy) {
            EnemyType = enemyType;
            ShipType = shipType;
            Level = level;
            Energy = energy;
        }
    }
}