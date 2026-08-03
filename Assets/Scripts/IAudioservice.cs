// 오디오 구현체를 인터페이스로 추상화 (구현 교체 시 GameManager 코드 변경 불필요)
public interface IAudioService
{
    void PlaySFX(string name);
}