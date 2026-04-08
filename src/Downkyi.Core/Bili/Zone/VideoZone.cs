namespace Downkyi.Core.Bili.Zone;

public class ZoneAttr
{
    public int Id { get; }
    public string EnName { get; }
    public string Name { get; }
    public int ParentId { get; }
    public ZoneAttr(int id, string enName, string name, int parentId = 0)
    { Id = id; EnName = enName; Name = name; ParentId = parentId; }
}

public static class VideoZone
{
    private static readonly List<ZoneAttr> _zones = BuildZones();

    public static List<ZoneAttr> GetZones() => _zones;

    /// <summary>
    /// Returns "Parent>Child" display name for a zone ID.
    /// Falls back to <paramref name="fallback"/> if not found.
    /// </summary>
    public static string GetZoneName(int tid, string fallback = "")
    {
        var zone = _zones.Find(z => z.Id == tid);
        if (zone == null) return fallback;
        var parent = _zones.Find(z => z.Id == zone.ParentId);
        return parent != null ? $"{parent.Name}>{zone.Name}" : zone.Name;
    }

    private static List<ZoneAttr> BuildZones() => new()
    {
        // 动画
        new(1, "douga", "动画"),
        new(24, "mad", "MAD·AMV", 1),
        new(25, "mmd", "MMD·3D", 1),
        new(47, "voice", "短片·手书·配音", 1),
        new(210, "garage_kit", "手办·模玩", 1),
        new(86, "tokusatsu", "特摄", 1),
        new(253, "acgntalks", "动漫杂谈", 1),
        new(27, "other", "综合", 1),
        // 番剧
        new(13, "anime", "番剧"),
        new(33, "serial", "连载动画", 13),
        new(32, "finish", "完结动画", 13),
        new(51, "information", "资讯", 13),
        new(152, "offical", "官方延伸", 13),
        // 国创
        new(167, "guochuang", "国创"),
        new(153, "chinese", "国产动画", 167),
        new(168, "original", "国产原创相关", 167),
        new(169, "puppetry", "布袋戏", 167),
        new(195, "motioncomic", "动态漫·广播剧", 167),
        new(170, "information", "资讯", 167),
        // 音乐
        new(3, "music", "音乐"),
        new(28, "original", "原创音乐", 3),
        new(31, "cover", "翻唱", 3),
        new(59, "perform", "演奏", 3),
        new(29, "mv", "MV", 3),
        new(54, "live", "音乐现场", 3),
        new(130, "vocaloid", "VOCALOID·UTAU", 3),
        new(243, "electronic", "电音", 3),
        new(30, "other", "音乐综合", 3),
        // 舞蹈
        new(129, "dance", "舞蹈"),
        new(20, "otaku", "宅舞", 129),
        new(154, "three_d", "舞蹈综合", 129),
        new(156, "demo", "舞蹈教程", 129),
        new(198, "hiphop", "街舞", 129),
        new(199, "star", "明星舞蹈", 129),
        new(200, "china", "中国舞", 129),
        // 游戏
        new(4, "game", "游戏"),
        new(17, "stand_alone", "单机游戏", 4),
        new(171, "esports", "电子竞技", 4),
        new(172, "mobile", "手机游戏", 4),
        new(65, "online", "网络游戏", 4),
        new(173, "board", "桌游棋牌", 4),
        new(121, "gmv", "GMV", 4),
        new(136, "music", "音游", 4),
        new(19, "mugen", "Mugen", 4),
        // 知识
        new(36, "knowledge", "知识"),
        new(201, "science", "科学科普", 36),
        new(124, "social_science", "社科·法律·心理", 36),
        new(228, "humanity_history", "人文历史", 36),
        new(207, "finance", "财经商业", 36),
        new(208, "campus", "校园学习", 36),
        new(209, "career", "职业职场", 36),
        new(229, "design", "设计·创意", 36),
        new(122, "skill", "野生技术协会", 36),
        // 科技
        new(188, "tech", "科技"),
        new(95, "digital", "数码", 188),
        new(230, "application", "软件应用", 188),
        new(231, "computer_tech", "计算机技术", 188),
        new(232, "industry", "科工机械", 188),
        new(233, "wireless", "极客DIY", 188),
        // 运动
        new(234, "sports", "运动"),
        new(235, "basketball", "篮球", 234),
        new(249, "football", "足球", 234),
        new(164, "aerobics", "健身", 234),
        new(236, "athletic", "竞技体育", 234),
        new(237, "culture", "运动文化", 234),
        new(238, "multiple", "运动综合", 234),
        // 汽车
        new(223, "car", "汽车"),
        new(245, "racing", "赛车", 223),
        new(246, "modifiedvehicle", "改装玩车", 223),
        new(247, "tips", "新能源车", 223),
        new(248, "cartourist", "房车", 223),
        new(240, "drafts", "汽车生活", 223),
        new(244, "culture", "汽车文化", 223),
        new(176, "record", "购车攻略", 223),
        // 生活
        new(160, "life", "生活"),
        new(138, "funny", "搞笑", 160),
        new(250, "travel", "出行", 160),
        new(251, "rurallife", "三农", 160),
        new(239, "home", "家居房产", 160),
        new(161, "handmake", "手工", 160),
        new(162, "painting", "绘画", 160),
        new(21, "daily", "日常", 160),
        // 美食
        new(211, "food", "美食"),
        new(76, "make", "美食制作", 211),
        new(212, "detective", "美食侦探", 211),
        new(213, "measurement", "美食测评", 211),
        new(214, "rural", "田园美食", 211),
        new(215, "record", "美食记录", 211),
        // 动物圈
        new(217, "animal", "动物圈"),
        new(218, "cat", "喵星人", 217),
        new(219, "dog", "汪星人", 217),
        new(220, "panda", "大熊猫", 217),
        new(221, "wild_animal", "野生动物", 217),
        new(222, "reptiles", "爬宠", 217),
        new(75, "other_animal", "动物综合", 217),
        // 鬼畜
        new(119, "kichiku", "鬼畜"),
        new(22, "guide", "鬼畜调教", 119),
        new(26, "mad", "音MAD", 119),
        new(126, "raps", "人力VOCALOID", 119),
        new(216, "movie_cut", "鬼畜剧场", 119),
        new(127, "other", "教程演示", 119),
        // 时尚
        new(155, "fashion", "时尚"),
        new(157, "makeup", "美妆护肤", 155),
        new(252, "cos", "仿妆cos", 155),
        new(158, "clothing", "穿搭", 155),
        new(159, "catwalk", "时尚潮流", 155),
        // 资讯
        new(202, "information", "资讯"),
        new(203, "hot", "热点", 202),
        new(204, "global", "环球", 202),
        new(205, "social", "社会", 202),
        new(206, "multiple", "综合", 202),
        // 娱乐
        new(5, "ent", "娱乐"),
        new(71, "variety", "综艺", 5),
        new(241, "fans", "粉丝创作", 5),
        new(242, "celebrity", "明星综合", 5),
        // 影视
        new(181, "cinephile", "影视"),
        new(182, "cinecism", "影视杂谈", 181),
        new(183, "montage", "影视剪辑", 181),
        new(85, "shortfilm", "小剧场", 181),
        new(184, "trailer_info", "预告·资讯", 181),
        // 纪录片
        new(177, "documentary", "纪录片"),
        new(37, "history", "历史", 177),
        new(178, "science", "科学", 177),
        new(179, "military", "军事", 177),
        new(180, "travel", "探险", 177),
        new(185, "animal", "自然", 177),
        new(186, "humanity_history", "人文", 177),
        // 电影
        new(23, "movie", "电影"),
        new(147, "chinese", "华语电影", 23),
        new(145, "west", "欧美电影", 23),
        new(146, "japan", "日本电影", 23),
        new(83, "documentary", "纪录片", 23),
        new(187, "other", "其他国家", 23),
        // 电视剧
        new(11, "teleplay", "电视剧"),
        new(189, "mainland", "国产剧", 11),
        new(190, "overseas", "海外剧", 11),
    };
}
