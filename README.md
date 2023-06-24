# Excel Database

Excel Database는 엑셀로 작성된 테이블 데이터를 JSON 형식으로 파싱하고, 파싱한 테이블에 대한 C# 스크립트를 생성해주는
라이브러리입니다. 이 라이브러리를 사용하면 엑셀에서 작성한 테이블을 손쉽게 파싱하여 유니티 프로젝트에서 활용할 수 있습니다.

## Getting Started

1. [Releases](https://github.com/AlyxerHL/ExcelDatabase/releases)에서 최신 버전의 라이브러리를 다운로드합니다.
2. 다운로드한 파일을 압축 해제합니다.
3. 압축 해제한 파일을 유니티 프로젝트의 `Assets/Plugins` 폴더에 복사합니다.

## Tables

Excel Database는 다음과 같은 종류의 테이블을 지원합니다.

### Convert Table

Convert 테이블은 일반적인 데이터 테이블로, 엑셀에서 여러 열로 구성되어 있는 데이터를 행 단위로 정리한 형태입니다.
Convert 테이블은 주로 게임의 아이템 데이터, 캐릭터 속성 데이터, 레벨 정보 데이터 등을 저장하는 데 사용됩니다.

### Enum Table

Enum 테이블은 엑셀에서 작성된 Enum 형식의 데이터를 담고 있는 테이블입니다.
Enum은 상수 값을 정의하는 데이터 형식으로, 게임 내에서 사용되는 여러 상태나 유형을 표현할 때 자주 활용됩니다.
Enum 테이블은 각 상수 값과 해당 값의 이름을 매칭하여 정의합니다.

### Design Variable Table

Design Variable 테이블은 상수 값을 저장하는 테이블로, 게임의 디자인 변수나 설정 값을 정의합니다.
이 테이블은 게임의 튜닝 가능한 값들을 효율적으로 관리하고 수정할 수 있도록 도와줍니다.
Design Variable 테이블은 게임의 밸런스 조정이나 다양한 설정 값을 관리하는 데 유용합니다.

## Features

Excel Database는 다음과 같은 기능을 제공합니다.

### Menu Item

Excel Database는 메뉴 아이템으로 새로운 테이블을 파싱하여 추가하는 기능을 제공합니다.
또한, 테이블 리스트를 열어서 파싱된 테이블들의 데이터를 관리할 수 있습니다.

### Table List

테이블 리스트는 파싱된 테이블 데이터를 관리하는 기능을 제공합니다. 테이블 리스트에서는 다음과 같은 작업을 수행할 수 있습니다.

-   JSON 에디터 열기: Convert 테이블에 대한 JSON 데이터를 편집할 수 있는 에디터를 열 수 있습니다.
-   테이블 다시 파싱: 테이블 데이터를 다시 파싱하여 업데이트할 수 있습니다.
-   테이블 데이터 삭제: 파싱된 테이블 데이터를 삭제할 수 있습니다.

### JSON Editor

JSON 에디터는 Convert 테이블의 JSON 데이터를 편집하는 기능을 제공합니다.
이 에디터를 통해 Convert 테이블의 데이터를 수정하고 저장할 수 있습니다.

## Usage

-   Convert 테이블: `string value = Tb.TableName["ID"].ColumnName;`
-   Convert 테이블 LINQ:
    `IEnumerable<Tb.TableNameType> value = Tb.TableName.Where((row) => row.Group == "SomeGroup");`
-   Enum 테이블: `Em.TableName.EnumName value = Em.TableName.EnumName.None;`
-   Design Variable 테이블: `string value = Dv.TableName.VariableName;`
