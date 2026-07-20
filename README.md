# 技术栈

1. 开发工具：Unity 2022.3.62f3 + VSCode, Visual Studio
2. 热更新：HybridCLR，YooAsset
3. 数据持久化：ScriptableObject，JSON，PlayerPrefs
4. 插件：DOTween，InputSystem，Cinemachine

# 核心功能

1. 热更新：使用HybridCLR和YooAsset分别实现代码热更新和资源热更新，并依托Unity的UOS实现公网热更新监测和下载。基于YooAsset实现五种情况下的游戏运行方式：
     1. 网络良好且有更新时，玩家选择下载
     2. 网络良好且有更新时，玩家取消下载（使用本地游戏版本）
     3. 网络良好且无更新（本地版本已经最新）
     4. 无网络（使用本地游戏版本）
     5. Unity Editor Simulate Mode
2. 存档系统：使用JSON进行玩家数据的存档、读档以及游戏全局设置的保存。
3. 背包系统：
     1. 使用ScriptableObject进行物品的配置，并实现物品的存放、排序、丢弃和查看详情
     2. 与交易系统、盲盒系统、装备强化系统和副本战斗系统进行数据互通
     3. 使用DOTween实现物品选择时UI淡入淡出的交互效果
4. 任务系统：通过任务栏页面可以进行任务详情查看、任务进度监测 和 任务奖励的领取，与其他各系统进行数据互通，实时监测任务进度。
5. 对话系统：按照游戏流程，将玩家与每一个NPC的对话逐个展开，便于进行游戏流程引导和NPC功能划分，并使用DOTween实现玩家与NPC对话时的文本逐字显示效果。
6. 交易系统：实现了直接用金币购买物品和用金币进行开盲盒的抽奖部分，使用DOTween实现开启盲盒时的动画效果。
7. 装备强化系统：将装备分为一个武器部位和五个防具部位，可以分别进行强化，强化时需要找到铁匠NPC并消耗对应的材料和金币才可以进行强化。
8. 动作系统：
     1. 使用泛型FSM、InputSystem和Cinemachine实现第三人称的角色控制
     2. 通过ScriptableObject、Animator和代码逻辑实现攻击和技能的配置和释放
     3. 实现轻攻击连招、派生重攻击、普通重攻击、跳跃斩击、目标锁定等功能
     4. 使用Blend Tree实现八方向的角色移动和四方向的翻滚、闪避
     5. 在角色使用三种重攻击时，角色处于霸体状态
9. 战斗系统：
     1. 使用对象池动态生成敌人，并依照波次生成敌人
     2. 使用泛型有限状态机控制敌人的行为，包含Idle、巡逻、追逐、攻击、受击、死亡状态
     3. 角色进入副本开始战斗，战斗失败可以重试或直接返回村庄，战斗胜利则结算奖励，然后返回村庄
     4. 角色具有暴击率和闪避率属性，在与敌人的战斗中，触发暴击可以对敌人造成两倍伤害，触发闪避让敌人的本次攻击伤害为0
10. 性能优化
     1. 静态和动态批处理
     2. GPU Instancing：将使用相同材质的物体的材质球启用GPU Instancing，提升合并的批处理数量，从而减少Batches。
     3. 遮挡剔除
     4. 光照烘焙：将静态物体进行Contribute GI勾选设置，设置光源的Light组件的Mode为Mixed，然后Generate Lighting，极大降低Batches和SetPass calls，并保留动态物体（Player、Enemy）的阴影；缺点就是占用内存，用空间换时间。
      5. 对象池：将Player闪避后的分身效果以及Enemy的生成采用对象池生成。

# 性能优化（详细版）

## 一、Draw Call 优化

### 1. 静态批处理
- 将场景中不移动的物体（墙壁、地板、天花板、火把等）勾选 Batching Static
- Unity 自动合并使用相同材质的静态物体到同一批次渲染
- 降低 CPU 提交 Draw Call 的次数

### 2. 动态合批
- 针对动态物体，勾选后自动生效
- 使用相同材质的动态物体可合并到同一批次

### 3. GPU Instancing
- 在材质面板勾选 Enable GPU Instancing
- 在一个 Draw Call 中同时渲染多个相同或类似的物体，减少 CPU/GPU 性能开销
- 代价是占用一定内存，适合场景中大量重复物体（如火把、旗帜）
- 本项目 DungeonScene 中大量重复的 Torch、Flag 等物体受益于此

### 4. 遮挡剔除
- 将静态物体勾选 Occluder Static 和 Occludee Static
- 相机看不到的物体不进行渲染
- DungeonScene 作为室内封闭场景，遮挡剔除效果显著
- 通过 Window → Rendering → Occlusion Culling → Visualization 可视化验证
- 通过 Stats 窗口对比 Batches、Tris、SetPass Calls 的下降

## 二、光照优化

### 1. 光照烘焙
- 将 Directional Light 和 Point Light 的 Mode 设为 Mixed
- 静态物体勾选 Contribute GI，烘焙后间接光和静态阴影固化到光照贴图
- Lighting Mode 设为 Shadowmask：静态阴影来自烘焙贴图，动态物体（Player、Enemy）的实时阴影来自实时计算
- 极大降低运行时 Batches 和 SetPass Calls，用磁盘空间换运行时性能
- 调参建议：Indirect Intensity 降到 0.3~0.5、Max Bounces 从 2 降到 1，可缩小烘焙前后视觉差异

### 2. 光照贴图配置
- Lightmap Resolution: 40 texels/unit
- Max Lightmap Size: 1024
- Lightmap Compression: High Quality
- Directional Mode: Directional（保留法线贴图的方向光照信息）

## 三、对象池

### 1. Enemy 对象池
- `EnemySpawner` 预实例化指定数量的 Enemy，通过 Spawn/Despawn 复用
- 避免运行时频繁 Instantiate/Destroy 造成的 GC 开销和卡顿
- 三波次生成（1 → 3 → 5），Fisher-Yates 随机选点

### 2. Player 残影对象池
- `AfterImageEffect` 预实例化残影对象
- 闪避时从池中取出，用完归还，避免每帧动态创建

## 四、音频优化

### 1. 异步加载
- `AudioService` 使用 YooAsset `LoadAssetAsync` 异步加载 AudioClip
- 避免同步加载造成主线程卡顿
- Dictionary 缓存已加载的 AudioClip，避免重复加载

### 2. 三通道音量控制
- Master/BGM/SFX 独立控制，设置页实时调节并持久化

## 五、Stats 窗口关键指标

| 指标 | 含义 | 优化目标 |
|------|------|----------|
| FPS | 帧率 | ≥ 60，不低于 30 |
| Batches | 绘制批次总数 | 越少越好 |
| SetPass Calls | 渲染通道切换次数 | 越少越好 |
| Tris | 三角面数 | 越少越好 |
| Saved by Batching | 被合并的批次 | 越多越好 |
| Shadow Casters | 产生阴影的物体数 | 避免过多 |