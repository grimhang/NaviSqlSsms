# DaviSql for SSMS

DaviSql for SSMS은 SQL Server Management Studio (SSMS) 사용을 돕는 확장기능이다.


# 기능
- Sql Selector : SSMS 편집기에서 커서의 위치 쿼리만 선택 하거나 실행할수 있는 확장기능. [ssms-executor](https://github.com/devvcat/ssms-executor)를 참고함.

- Lang AutoFix : 한글로 강제로 바뀌는 버그 수정. 

## 설치
[Download](https://github.com/grimhang/DaviSqlSsms/releases/download/V0.9/DaviSqlSsms_V0.9.2.zip)

압축을 풀고 DaviSqlSsms 폴더를  다음 폴더에 붙여넣기  
 C:\Program Files (x86)\Microsoft SQL Server Management Studio 19\Common7\IDE\Extensions\  


복사할때 관리자 권한 물어볼수 있음.

현재 SSMS 19만 테스트해봄


## 사용법

### Sql Selector
    SSMS에 DaviSql 라는 메뉴가 생긴다.  

    Ctrl + Shift + 엔터를 치면 커서위치 sql만 선택. 한번더 치면 sql 실행.

### Lang AutoFix
    DaviSql 메뉴에 Lang AutoFix 클릭하면 알림팝업이 뜨고 이제부터 한글로 고정되는 문제 자동 해결.  
    
    변경 실시간 로그를 보려면 메뉴에서 보기 / 출력(Ctrl + Alt + O) 클릭. 
    아래쪽에 출력 Windows가 나오고 출력 보기 선택 드롭다운에서 DaviSql Ssms 선택.