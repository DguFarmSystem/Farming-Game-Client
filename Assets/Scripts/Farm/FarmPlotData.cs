

[System.Serializable]
public class FarmPlotData
{
    public string plot_id; // 땅 아이디
    public int x; // x 좌표
    public int y; // y 좌표
    public string plant_name; // 작물 이름
    public string planted_at; // 심은 시간
    public bool is_shiny; //이로치 여부
    public string status; //상태 "empty", "growing", "grown"
    public string uid; //유저 아이디

    public int useSunCount; //햇살 사용수 필요할거같음
}
