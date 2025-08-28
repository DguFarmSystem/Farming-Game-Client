

using System;

[System.Serializable]
public class FarmPlotData
{
    public int x; // 땅아이디
    public int y; // 땅아이디
    public string plant_name; // 작물 이름
    public DateTime planted_at; // 심은 시간
    public string status; //상태 "empty", "growing", "grown"
    public string uid; //유저 아이디

    public int useSunCount; //햇살 사용수 필요할거같음
}
