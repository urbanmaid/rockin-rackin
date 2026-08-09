> English
    # Planning

    - Basic concept - Vampire Survivors + Super Monkey Ball

    ## Gameplay

    - The player is located on a tiltable field, and within the field there is an enemy entity that can deal damage to the player.
    The player's objects can be moved by the player manipulating the slope of the stage.
    The player and the enemies are in the same stage, and they are all affected by the same stage gravity. The enemy, likewise, is affected and moves under the influence of the tilted stage.
    Through tilting more, the tilt angle can be amplified while pressing.
    - The player object has a fixed amount of health, and after a certain amount is used up, it becomes game over.
    The player must keep moving and survive by eating health items.
        - The amount of stamina that can be fully replenished is limited.
        - The levels of changes in physical fitness are divided as follows.
            - Time-course decrease: slow and steady
            - Red contact damage: provides a brief but instantaneous invincibility period
            - Fall out of the field: return to the center after taking major damage
            - Health item: offsets the reduction rate or partially restores
    - If enemy objects are present around the player object, the push-out function can be used.
        - However, the push-back function requires a cooldown period of a certain amount of time.
    - When the stage is tilted, depending on the position of the central axis, players or enemy objects may suddenly lift off when the stage is tilted. This causes a floating effect.
        - Make use of this to enhance the sense of depth in gameplay. Tilting the player object to the center of the map, leaving only the enemy objects around the player object to float, so that the player can use the space lifted by enemy objects as evasion space.
        - Through manipulation, it provides the player with a way to indirectly attack by dealing fall damage to enemies. Enemy damage is applied only when they hit the stage at a speed above a certain level.
        When a certain amount of health is consumed, the enemy entity is destroyed and replaced with a health item.
    - Player objects can increase their score by consuming health items, and upon reaching a certain score, they can upgrade the following items. Three of the following items are randomly selected.
        - Stamina item density
        - Maximum health
        - Degree of leaning further
        - Push-out cooldown / power / range
        - Luck - Increases the drop rate of health items upon enemy defeat, increases the chance of high-rank upgrades appearing, and increases the chance of critical hits occurring from fall damage
        The camera is fixed so as to track the player while facing forward.

    ## Manipulation

    - Keyboard and mouse
        - WASD and arrow keys - tilt the stage, and some smoothing should be applied when tilting
        - Use the space - push function
        - Use Shift - Tilt More
    - Gamepad
        - L-Steak - Tilt the stage
        - Use the X - Push function
        - A - Use tilt more

    ## Theme

    - A combination of a fantasy theme and an alchemy theme
        - During the witch’s alchemical experiments, they succeed in creating a substance that absorbs matter and takes up its weight without increasing its volume, and they experiment with how much matter can be absorbed.
        - While other substances that interfere with the absorption of the substance fall together, we decide to increase the weight of the substance by avoiding them as much as possible.

    # Development Method

    - Generated in a local directory using ChatGPT 5.5, based on the plan specification.
    After reviewing the plan specifications and actual gameplay, it was developed while repeatedly requesting revisions for any differences from the play experience.
    Most of the game code and shader code use AI.
        - Some parts of the game code, as well as art assets, UI, and particles, do not use AI.

# Korean

# 기획

- 기본 컨셉 - 뱀파이어 서바이버 + 슈퍼 몽키볼

## 게임플레이

- 플레이어는 기울일 수 있는 필드에 존재하며, 필드 내에는 플레이어에게 데미지를 줄 수 있는 적 개체가 존재한다.
- 플레이어의 개체는 플레이어가 스테이지의 기울기를 조작해 움직일 수 있다.
    - 플레이어와 적은 같은 스테이지에 존재하며, 전부 동일한 스테이지 중력에 영향을 받는다. 적도 마찬가지로 기울어진 스테이지에 영향을 받아 움직인다.
    - 더 기울이기를 통해 기울이는 각을 누르는 동안 증폭시킬 수 있다.
- 플레이어 개체는 일정한 체력을 가지고 있으며, 시간이 지나면 소모되어 게임 오버된다.
    - 플레이어는 계속해서 움직이면서 체력 아이템을 먹어 가면서 버텨야 한다.
    - 체력은 최대로 채울 수 있는 양이 정해져 있다.
    - 체력 변화 수준은 다음과 같이 나뉜다.
        - 시간 경과 감소: 느리고 일정함
        - 적 접촉 피해: 순간적이지만 짧은 무적 시간 제공
        - 필드 밖 추락: 큰 피해 후 중앙 복귀
        - 체력 아이템: 감소 속도를 상쇄하거나 일부 회복
- 플레이어 개체 주변에 적 개체가 존재할 경우 밀어내기 기능을 사용할 수 있다.
    - 단, 밀어내기 기능은 일정 시간의 쿨타임을 요구한다.
- 스테이지를 기울인 상태에서 중심축의 위치에 따라 플레이어나 적 개체가 급격하게 스테이지를 기울였을 때 붕 뜨는 현상이 발생한다.
    - 이를 살려서 게임플레이에서의 입체감을 살린다. 플레이어 개체를 기울이는 맵의 중심으로 두고, 플레이어 개체 주변 적 개체만 붕 뜨도록 냅둬 플레이어가 적 개체에 의해 띄워진 공간을 회피 공간으로 쓸 수 있게 한다.
    - 조작을 통해 적들에게 낙하 데미지를 제공해 간접적으로 공격할 수 있는 방법을 플레이어에게 제공한다. 적들의 데미지는 일정 수준의 속도 이상으로 스테이지에 부딛혔을 경우에만 가해진다.
    - 일정 수준의 체력이 소모될 경우 적 개체는 파괴되며, 체력 아이템으로 변경된다.
- 플레이어 개체는 체력 아이템을 먹어 가면서 점수를 올릴 수 있으며, 일정 수준의 점수 도달 시 다음 사항을 업그레이드할 수 있다. 다음 사항 중 3개가 랜덤 선택된다.
    - 체력 아이템 밀도
    - 최대 체력
    - 더 기울이기 수준
    - 밀어내기 쿨타임, 밀어내기 세기, 밀어내기 영역 크기
    - 운 - 적 처치 시 체력 아이템 드롭 확률과 높은 등급 업그레이드 등장 확률 증가, 낙하 피해에 치명타가 발생할 확률 증가
- 카메라는 정방향을 바라보는 식으로 플레이어를 추적하도록 고정되어 있다.

## 조작

- 키보드 및 마우스
    - WASD, 화살표 - 스테이지 기울이기, 기울일 때 약간의 스무딩 적용 필요
    - 스페이스 - 밀어내기 기능 사용
    - 쉬프트 - 더 기울이기 사용
- 게임패드
    - L스틱 - 스테이지 기울이기
    - X - 밀어내기 기능 사용
    - A - 더 기울이기 사용

## 테마

- 판타지 테마와 연금술 테마의 조합
    - 마녀의 연금술 실험 과정에서 물질을 흡수해 부피를 늘리지 않고도 그 무게를 흡수하는 물질을 만드는 데 성공하며, 얼마나 많은 물질 흡수가 가능한지를 실험한다.
    - 도중에 물질의 흡수를 방해하는 다른 물질이 같이 떨어지면서 이들을 최대한 회피하면서 물질의 무게를 늘리기로 한다.

# 개발 방법

- ChatGPT 5.5를 사용한 로컬 디렉토리에서의 계획 명세서를 기반으로 생성.
    - 계획 명세서와 실제 게임플레이 검토 후, 플레이 경험과 차이가 나는 사항에 대한 수정을 반복적으로 요청하면서 제작.
    - 게임 코드 대부분과 쉐이더 코드는 AI 사용.
    - 게임 코드 일부분과 아트 어셋, UI, 파티클 등은 AI 미사용.