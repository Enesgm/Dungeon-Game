using UnityEngine;

public enum GameState
{
    Menu,       // Ana Menü
    Loading,    // Sahne/level yükleniyor
    Playing,    // Oyun oynanıyor
    Paused,     // Oyun duraklatıldı
    Victory,    // Oyunu kazandık
    GameOver    // Oyunu kaybettik
}