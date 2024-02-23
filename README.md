# DaviSql for SSMS
SQL Server Management Studio (SSMS) 사용을 돕는 확장기능.


# 기능
- Easy Select : SSMS 편집기에서 커서의 위치 쿼리만 선택 하거나 실행할수 있는 확장기능.                  
                    
    - Easy Select Advanced: SSMS 텍스트 편집기 본문중 에러가 있는 구문이 하나라도 존재하면 위의 Easy Select 작동 안함.
                    (TrasactSqlDom 구조상 당연)
                    이럴 경우 강제로 현재커서위치의 위/아래 공백라인까지만 선택됨.
                    
- AutoFix Lang : 한글로 강제로 바뀌는 SSMS버그 수정. (20년을 고생)

# 설치
[Download](https://github.com/grimhang/DaviSqlSsms/releases/download/V0.9.8/DaviSqlSsms_V0.9.8.zip)
           
    압축을 풀고 DaviSqlSsms 폴더를  다음 폴더에 붙여넣기  
     C:\Program Files (x86)\Microsoft SQL Server Management Studio 19\Common7\IDE\Extensions\  

    복사할때 관리자 권한 물어볼수도 있음.  
    현재 SSMS 19.x에서만 테스트해봄


# 사용법

#### 1. Easy Select
    SSMS에 DaviSql 라는 메뉴가 생긴다.  

    Ctrl + Shift + 엔터를 치면 커서위치 sql만 선택. 한번더 치면 sql 실행.

#### 2. AutoFix Lang
    DaviSql 메뉴에 Lang AutoFix 클릭하면 알림팝업이 뜸. 가끔씩 한글로 고정되는 SSMS문제 자동 교정.  

    실시간으로 교정되는지 보려면 메뉴 / 보기 / 출력(Ctrl + Alt + O) 클릭 후에  
     출력 Windows의 드롭다운에서 DaviSql Ssms 선택.