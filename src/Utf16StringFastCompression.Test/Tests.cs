using System.Runtime.InteropServices;

namespace Utf16StringFastCompression.Test;

public class Tests
{
    [Theory]
    [InlineData("abcdefgh")]
    [InlineData("abcdefghi")]
    [InlineData("abcdefghij")]
    [InlineData("abcdefghijk")]
    [InlineData("abcdefghijkl")]
    [InlineData("abcdefghijklm")]
    [InlineData("abcdefghijklmn")]
    [InlineData("abcdefghijklmno")]
    public void AllAsciiTest(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length + 2, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうえおかきく")]
    [InlineData("あいうえおかきくけ")]
    [InlineData("あいうえおかきくけこ")]
    public void AllUtf16Test(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうabcdefghijklmえおかきく")]
    [InlineData("あいうabcdefghijklmnえおかきくけ")]
    [InlineData("あいうabcdefghijklmnoえおかきくけこ")]
    [InlineData("ほるもんはdazydazyほうるもamazingんあいうabcdefghijklmえおかきく")]
    [InlineData("▓▓tinkle tinkle！！1！14→514↑1919810▓▓▓▓▓▓")]
    [InlineData("very short ascii text")]
    [InlineData("""
走れメロス
太宰治



　メロスは激怒した。必ず、かの邪智暴虐じゃちぼうぎゃくの王を除かなければならぬと決意した。メロスには政治がわからぬ。メロスは、村の牧人である。笛を吹き、羊と遊んで暮して来た。けれども邪悪に対しては、人一倍に敏感であった。きょう未明メロスは村を出発し、野を越え山越え、十里はなれた此このシラクスの市にやって来た。メロスには父も、母も無い。女房も無い。十六の、内気な妹と二人暮しだ。この妹は、村の或る律気な一牧人を、近々、花婿はなむことして迎える事になっていた。結婚式も間近かなのである。メロスは、それゆえ、花嫁の衣裳やら祝宴の御馳走やらを買いに、はるばる市にやって来たのだ。先ず、その品々を買い集め、それから都の大路をぶらぶら歩いた。メロスには竹馬の友があった。セリヌンティウスである。今は此のシラクスの市で、石工をしている。その友を、これから訪ねてみるつもりなのだ。久しく逢わなかったのだから、訪ねて行くのが楽しみである。歩いているうちにメロスは、まちの様子を怪しく思った。ひっそりしている。もう既に日も落ちて、まちの暗いのは当りまえだが、けれども、なんだか、夜のせいばかりでは無く、市全体が、やけに寂しい。のんきなメロスも、だんだん不安になって来た。路で逢った若い衆をつかまえて、何かあったのか、二年まえに此の市に来たときは、夜でも皆が歌をうたって、まちは賑やかであった筈はずだが、と質問した。若い衆は、首を振って答えなかった。しばらく歩いて老爺ろうやに逢い、こんどはもっと、語勢を強くして質問した。老爺は答えなかった。メロスは両手で老爺のからだをゆすぶって質問を重ねた。老爺は、あたりをはばかる低声で、わずか答えた。
「王様は、人を殺します。」
「なぜ殺すのだ。」
「悪心を抱いている、というのですが、誰もそんな、悪心を持っては居りませぬ。」
「たくさんの人を殺したのか。」
「はい、はじめは王様の妹婿さまを。それから、御自身のお世嗣よつぎを。それから、妹さまを。それから、妹さまの御子さまを。それから、皇后さまを。それから、賢臣のアレキス様を。」
「おどろいた。国王は乱心か。」
「いいえ、乱心ではございませぬ。人を、信ずる事が出来ぬ、というのです。このごろは、臣下の心をも、お疑いになり、少しく派手な暮しをしている者には、人質ひとりずつ差し出すことを命じて居ります。御命令を拒めば十字架にかけられて、殺されます。きょうは、六人殺されました。」
　聞いて、メロスは激怒した。「呆あきれた王だ。生かして置けぬ。」
　メロスは、単純な男であった。買い物を、背負ったままで、のそのそ王城にはいって行った。たちまち彼は、巡邏じゅんらの警吏に捕縛された。調べられて、メロスの懐中からは短剣が出て来たので、騒ぎが大きくなってしまった。メロスは、王の前に引き出された。
「この短刀で何をするつもりであったか。言え！」暴君ディオニスは静かに、けれども威厳を以もって問いつめた。その王の顔は蒼白そうはくで、眉間みけんの皺しわは、刻み込まれたように深かった。
「市を暴君の手から救うのだ。」とメロスは悪びれずに答えた。
「おまえがか？」王は、憫笑びんしょうした。「仕方の無いやつじゃ。おまえには、わしの孤独がわからぬ。」
「言うな！」とメロスは、いきり立って反駁はんばくした。「人の心を疑うのは、最も恥ずべき悪徳だ。王は、民の忠誠をさえ疑って居られる。」
「疑うのが、正当の心構えなのだと、わしに教えてくれたのは、おまえたちだ。人の心は、あてにならない。人間は、もともと私慾のかたまりさ。信じては、ならぬ。」暴君は落着いて呟つぶやき、ほっと溜息ためいきをついた。「わしだって、平和を望んでいるのだが。」
「なんの為の平和だ。自分の地位を守る為か。」こんどはメロスが嘲笑した。「罪の無い人を殺して、何が平和だ。」
「だまれ、下賤げせんの者。」王は、さっと顔を挙げて報いた。「口では、どんな清らかな事でも言える。わしには、人の腹綿の奥底が見え透いてならぬ。おまえだって、いまに、磔はりつけになってから、泣いて詫わびたって聞かぬぞ。」
「ああ、王は悧巧りこうだ。自惚うぬぼれているがよい。私は、ちゃんと死ぬる覚悟で居るのに。命乞いなど決してしない。ただ、――」と言いかけて、メロスは足もとに視線を落し瞬時ためらい、「ただ、私に情をかけたいつもりなら、処刑までに三日間の日限を与えて下さい。たった一人の妹に、亭主を持たせてやりたいのです。三日のうちに、私は村で結婚式を挙げさせ、必ず、ここへ帰って来ます。」
「ばかな。」と暴君は、嗄しわがれた声で低く笑った。「とんでもない嘘うそを言うわい。逃がした小鳥が帰って来るというのか。」
「そうです。帰って来るのです。」メロスは必死で言い張った。「私は約束を守ります。私を、三日間だけ許して下さい。妹が、私の帰りを待っているのだ。そんなに私を信じられないならば、よろしい、この市にセリヌンティウスという石工がいます。私の無二の友人だ。あれを、人質としてここに置いて行こう。私が逃げてしまって、三日目の日暮まで、ここに帰って来なかったら、あの友人を絞め殺して下さい。たのむ、そうして下さい。」
　それを聞いて王は、残虐な気持で、そっと北叟笑ほくそえんだ。生意気なことを言うわい。どうせ帰って来ないにきまっている。この嘘つきに騙だまされた振りして、放してやるのも面白い。そうして身代りの男を、三日目に殺してやるのも気味がいい。人は、これだから信じられぬと、わしは悲しい顔して、その身代りの男を磔刑に処してやるのだ。世の中の、正直者とかいう奴輩やつばらにうんと見せつけてやりたいものさ。
「願いを、聞いた。その身代りを呼ぶがよい。三日目には日没までに帰って来い。おくれたら、その身代りを、きっと殺すぞ。ちょっとおくれて来るがいい。おまえの罪は、永遠にゆるしてやろうぞ。」
「なに、何をおっしゃる。」
「はは。いのちが大事だったら、おくれて来い。おまえの心は、わかっているぞ。」
　メロスは口惜しく、地団駄じだんだ踏んだ。ものも言いたくなくなった。
　竹馬の友、セリヌンティウスは、深夜、王城に召された。暴君ディオニスの面前で、佳よき友と佳き友は、二年ぶりで相逢うた。メロスは、友に一切の事情を語った。セリヌンティウスは無言で首肯うなずき、メロスをひしと抱きしめた。友と友の間は、それでよかった。セリヌンティウスは、縄打たれた。メロスは、すぐに出発した。初夏、満天の星である。
　メロスはその夜、一睡もせず十里の路を急ぎに急いで、村へ到着したのは、翌あくる日の午前、陽は既に高く昇って、村人たちは野に出て仕事をはじめていた。メロスの十六の妹も、きょうは兄の代りに羊群の番をしていた。よろめいて歩いて来る兄の、疲労困憊こんぱいの姿を見つけて驚いた。そうして、うるさく兄に質問を浴びせた。
「なんでも無い。」メロスは無理に笑おうと努めた。「市に用事を残して来た。またすぐ市に行かなければならぬ。あす、おまえの結婚式を挙げる。早いほうがよかろう。」
　妹は頬をあからめた。
「うれしいか。綺麗きれいな衣裳も買って来た。さあ、これから行って、村の人たちに知らせて来い。結婚式は、あすだと。」
　メロスは、また、よろよろと歩き出し、家へ帰って神々の祭壇を飾り、祝宴の席を調え、間もなく床に倒れ伏し、呼吸もせぬくらいの深い眠りに落ちてしまった。
　眼が覚めたのは夜だった。メロスは起きてすぐ、花婿の家を訪れた。そうして、少し事情があるから、結婚式を明日にしてくれ、と頼んだ。婿の牧人は驚き、それはいけない、こちらには未だ何の仕度も出来ていない、葡萄ぶどうの季節まで待ってくれ、と答えた。メロスは、待つことは出来ぬ、どうか明日にしてくれ給え、と更に押してたのんだ。婿の牧人も頑強であった。なかなか承諾してくれない。夜明けまで議論をつづけて、やっと、どうにか婿をなだめ、すかして、説き伏せた。結婚式は、真昼に行われた。新郎新婦の、神々への宣誓が済んだころ、黒雲が空を覆い、ぽつりぽつり雨が降り出し、やがて車軸を流すような大雨となった。祝宴に列席していた村人たちは、何か不吉なものを感じたが、それでも、めいめい気持を引きたて、狭い家の中で、むんむん蒸し暑いのも怺こらえ、陽気に歌をうたい、手を拍うった。メロスも、満面に喜色を湛たたえ、しばらくは、王とのあの約束をさえ忘れていた。祝宴は、夜に入っていよいよ乱れ華やかになり、人々は、外の豪雨を全く気にしなくなった。メロスは、一生このままここにいたい、と思った。この佳い人たちと生涯暮して行きたいと願ったが、いまは、自分のからだで、自分のものでは無い。ままならぬ事である。メロスは、わが身に鞭打ち、ついに出発を決意した。あすの日没までには、まだ十分の時が在る。ちょっと一眠りして、それからすぐに出発しよう、と考えた。その頃には、雨も小降りになっていよう。少しでも永くこの家に愚図愚図とどまっていたかった。メロスほどの男にも、やはり未練の情というものは在る。今宵呆然、歓喜に酔っているらしい花嫁に近寄り、
「おめでとう。私は疲れてしまったから、ちょっとご免こうむって眠りたい。眼が覚めたら、すぐに市に出かける。大切な用事があるのだ。私がいなくても、もうおまえには優しい亭主があるのだから、決して寂しい事は無い。おまえの兄の、一ばんきらいなものは、人を疑う事と、それから、嘘をつく事だ。おまえも、それは、知っているね。亭主との間に、どんな秘密でも作ってはならぬ。おまえに言いたいのは、それだけだ。おまえの兄は、たぶん偉い男なのだから、おまえもその誇りを持っていろ。」
　花嫁は、夢見心地で首肯うなずいた。メロスは、それから花婿の肩をたたいて、
「仕度の無いのはお互さまさ。私の家にも、宝といっては、妹と羊だけだ。他には、何も無い。全部あげよう。もう一つ、メロスの弟になったことを誇ってくれ。」
　花婿は揉もみ手して、てれていた。メロスは笑って村人たちにも会釈えしゃくして、宴席から立ち去り、羊小屋にもぐり込んで、死んだように深く眠った。
　眼が覚めたのは翌る日の薄明の頃である。メロスは跳ね起き、南無三、寝過したか、いや、まだまだ大丈夫、これからすぐに出発すれば、約束の刻限までには十分間に合う。きょうは是非とも、あの王に、人の信実の存するところを見せてやろう。そうして笑って磔の台に上ってやる。メロスは、悠々と身仕度をはじめた。雨も、いくぶん小降りになっている様子である。身仕度は出来た。さて、メロスは、ぶるんと両腕を大きく振って、雨中、矢の如く走り出た。
　私は、今宵、殺される。殺される為に走るのだ。身代りの友を救う為に走るのだ。王の奸佞かんねい邪智を打ち破る為に走るのだ。走らなければならぬ。そうして、私は殺される。若い時から名誉を守れ。さらば、ふるさと。若いメロスは、つらかった。幾度か、立ちどまりそうになった。えい、えいと大声挙げて自身を叱りながら走った。村を出て、野を横切り、森をくぐり抜け、隣村に着いた頃には、雨も止やみ、日は高く昇って、そろそろ暑くなって来た。メロスは額ひたいの汗をこぶしで払い、ここまで来れば大丈夫、もはや故郷への未練は無い。妹たちは、きっと佳い夫婦になるだろう。私には、いま、なんの気がかりも無い筈だ。まっすぐに王城に行き着けば、それでよいのだ。そんなに急ぐ必要も無い。ゆっくり歩こう、と持ちまえの呑気のんきさを取り返し、好きな小歌をいい声で歌い出した。ぶらぶら歩いて二里行き三里行き、そろそろ全里程の半ばに到達した頃、降って湧わいた災難、メロスの足は、はたと、とまった。見よ、前方の川を。きのうの豪雨で山の水源地は氾濫はんらんし、濁流滔々とうとうと下流に集り、猛勢一挙に橋を破壊し、どうどうと響きをあげる激流が、木葉微塵こっぱみじんに橋桁はしげたを跳ね飛ばしていた。彼は茫然と、立ちすくんだ。あちこちと眺めまわし、また、声を限りに呼びたててみたが、繋舟けいしゅうは残らず浪に浚さらわれて影なく、渡守りの姿も見えない。流れはいよいよ、ふくれ上り、海のようになっている。メロスは川岸にうずくまり、男泣きに泣きながらゼウスに手を挙げて哀願した。「ああ、鎮しずめたまえ、荒れ狂う流れを！　時は刻々に過ぎて行きます。太陽も既に真昼時です。あれが沈んでしまわぬうちに、王城に行き着くことが出来なかったら、あの佳い友達が、私のために死ぬのです。」
　濁流は、メロスの叫びをせせら笑う如く、ますます激しく躍り狂う。浪は浪を呑み、捲き、煽あおり立て、そうして時は、刻一刻と消えて行く。今はメロスも覚悟した。泳ぎ切るより他に無い。ああ、神々も照覧あれ！　濁流にも負けぬ愛と誠の偉大な力を、いまこそ発揮して見せる。メロスは、ざんぶと流れに飛び込み、百匹の大蛇のようにのた打ち荒れ狂う浪を相手に、必死の闘争を開始した。満身の力を腕にこめて、押し寄せ渦巻き引きずる流れを、なんのこれしきと掻かきわけ掻きわけ、めくらめっぽう獅子奮迅の人の子の姿には、神も哀れと思ったか、ついに憐愍れんびんを垂れてくれた。押し流されつつも、見事、対岸の樹木の幹に、すがりつく事が出来たのである。ありがたい。メロスは馬のように大きな胴震いを一つして、すぐにまた先きを急いだ。一刻といえども、むだには出来ない。陽は既に西に傾きかけている。ぜいぜい荒い呼吸をしながら峠をのぼり、のぼり切って、ほっとした時、突然、目の前に一隊の山賊が躍り出た。
「待て。」
「何をするのだ。私は陽の沈まぬうちに王城へ行かなければならぬ。放せ。」
「どっこい放さぬ。持ちもの全部を置いて行け。」
「私にはいのちの他には何も無い。その、たった一つの命も、これから王にくれてやるのだ。」
「その、いのちが欲しいのだ。」
「さては、王の命令で、ここで私を待ち伏せしていたのだな。」
　山賊たちは、ものも言わず一斉に棍棒こんぼうを振り挙げた。メロスはひょいと、からだを折り曲げ、飛鳥の如く身近かの一人に襲いかかり、その棍棒を奪い取って、
「気の毒だが正義のためだ！」と猛然一撃、たちまち、三人を殴り倒し、残る者のひるむ隙すきに、さっさと走って峠を下った。一気に峠を駈け降りたが、流石さすがに疲労し、折から午後の灼熱しゃくねつの太陽がまともに、かっと照って来て、メロスは幾度となく眩暈めまいを感じ、これではならぬ、と気を取り直しては、よろよろ二、三歩あるいて、ついに、がくりと膝を折った。立ち上る事が出来ぬのだ。天を仰いで、くやし泣きに泣き出した。ああ、あ、濁流を泳ぎ切り、山賊を三人も撃ち倒し韋駄天いだてん、ここまで突破して来たメロスよ。真の勇者、メロスよ。今、ここで、疲れ切って動けなくなるとは情無い。愛する友は、おまえを信じたばかりに、やがて殺されなければならぬ。おまえは、稀代きたいの不信の人間、まさしく王の思う壺つぼだぞ、と自分を叱ってみるのだが、全身萎なえて、もはや芋虫いもむしほどにも前進かなわぬ。路傍の草原にごろりと寝ころがった。身体疲労すれば、精神も共にやられる。もう、どうでもいいという、勇者に不似合いな不貞腐ふてくされた根性が、心の隅に巣喰った。私は、これほど努力したのだ。約束を破る心は、みじんも無かった。神も照覧、私は精一ぱいに努めて来たのだ。動けなくなるまで走って来たのだ。私は不信の徒では無い。ああ、できる事なら私の胸を截たち割って、真紅の心臓をお目に掛けたい。愛と信実の血液だけで動いているこの心臓を見せてやりたい。けれども私は、この大事な時に、精も根も尽きたのだ。私は、よくよく不幸な男だ。私は、きっと笑われる。私の一家も笑われる。私は友を欺あざむいた。中途で倒れるのは、はじめから何もしないのと同じ事だ。ああ、もう、どうでもいい。これが、私の定った運命なのかも知れない。セリヌンティウスよ、ゆるしてくれ。君は、いつでも私を信じた。私も君を、欺かなかった。私たちは、本当に佳い友と友であったのだ。いちどだって、暗い疑惑の雲を、お互い胸に宿したことは無かった。いまだって、君は私を無心に待っているだろう。ああ、待っているだろう。ありがとう、セリヌンティウス。よくも私を信じてくれた。それを思えば、たまらない。友と友の間の信実は、この世で一ばん誇るべき宝なのだからな。セリヌンティウス、私は走ったのだ。君を欺くつもりは、みじんも無かった。信じてくれ！　私は急ぎに急いでここまで来たのだ。濁流を突破した。山賊の囲みからも、するりと抜けて一気に峠を駈け降りて来たのだ。私だから、出来たのだよ。ああ、この上、私に望み給うな。放って置いてくれ。どうでも、いいのだ。私は負けたのだ。だらしが無い。笑ってくれ。王は私に、ちょっとおくれて来い、と耳打ちした。おくれたら、身代りを殺して、私を助けてくれると約束した。私は王の卑劣を憎んだ。けれども、今になってみると、私は王の言うままになっている。私は、おくれて行くだろう。王は、ひとり合点して私を笑い、そうして事も無く私を放免するだろう。そうなったら、私は、死ぬよりつらい。私は、永遠に裏切者だ。地上で最も、不名誉の人種だ。セリヌンティウスよ、私も死ぬぞ。君と一緒に死なせてくれ。君だけは私を信じてくれるにちがい無い。いや、それも私の、ひとりよがりか？　ああ、もういっそ、悪徳者として生き伸びてやろうか。村には私の家が在る。羊も居る。妹夫婦は、まさか私を村から追い出すような事はしないだろう。正義だの、信実だの、愛だの、考えてみれば、くだらない。人を殺して自分が生きる。それが人間世界の定法ではなかったか。ああ、何もかも、ばかばかしい。私は、醜い裏切り者だ。どうとも、勝手にするがよい。やんぬる哉かな。――四肢を投げ出して、うとうと、まどろんでしまった。
　ふと耳に、潺々せんせん、水の流れる音が聞えた。そっと頭をもたげ、息を呑んで耳をすました。すぐ足もとで、水が流れているらしい。よろよろ起き上って、見ると、岩の裂目から滾々こんこんと、何か小さく囁ささやきながら清水が湧き出ているのである。その泉に吸い込まれるようにメロスは身をかがめた。水を両手で掬すくって、一くち飲んだ。ほうと長い溜息が出て、夢から覚めたような気がした。歩ける。行こう。肉体の疲労恢復かいふくと共に、わずかながら希望が生れた。義務遂行の希望である。わが身を殺して、名誉を守る希望である。斜陽は赤い光を、樹々の葉に投じ、葉も枝も燃えるばかりに輝いている。日没までには、まだ間がある。私を、待っている人があるのだ。少しも疑わず、静かに期待してくれている人があるのだ。私は、信じられている。私の命なぞは、問題ではない。死んでお詫び、などと気のいい事は言って居られぬ。私は、信頼に報いなければならぬ。いまはただその一事だ。走れ！　メロス。
　私は信頼されている。私は信頼されている。先刻の、あの悪魔の囁きは、あれは夢だ。悪い夢だ。忘れてしまえ。五臓が疲れているときは、ふいとあんな悪い夢を見るものだ。メロス、おまえの恥ではない。やはり、おまえは真の勇者だ。再び立って走れるようになったではないか。ありがたい！　私は、正義の士として死ぬ事が出来るぞ。ああ、陽が沈む。ずんずん沈む。待ってくれ、ゼウスよ。私は生れた時から正直な男であった。正直な男のままにして死なせて下さい。
　路行く人を押しのけ、跳はねとばし、メロスは黒い風のように走った。野原で酒宴の、その宴席のまっただ中を駈け抜け、酒宴の人たちを仰天させ、犬を蹴けとばし、小川を飛び越え、少しずつ沈んでゆく太陽の、十倍も早く走った。一団の旅人と颯さっとすれちがった瞬間、不吉な会話を小耳にはさんだ。「いまごろは、あの男も、磔にかかっているよ。」ああ、その男、その男のために私は、いまこんなに走っているのだ。その男を死なせてはならない。急げ、メロス。おくれてはならぬ。愛と誠の力を、いまこそ知らせてやるがよい。風態なんかは、どうでもいい。メロスは、いまは、ほとんど全裸体であった。呼吸も出来ず、二度、三度、口から血が噴き出た。見える。はるか向うに小さく、シラクスの市の塔楼が見える。塔楼は、夕陽を受けてきらきら光っている。
「ああ、メロス様。」うめくような声が、風と共に聞えた。
「誰だ。」メロスは走りながら尋ねた。
「フィロストラトスでございます。貴方のお友達セリヌンティウス様の弟子でございます。」その若い石工も、メロスの後について走りながら叫んだ。「もう、駄目でございます。むだでございます。走るのは、やめて下さい。もう、あの方かたをお助けになることは出来ません。」
「いや、まだ陽は沈まぬ。」
「ちょうど今、あの方が死刑になるところです。ああ、あなたは遅かった。おうらみ申します。ほんの少し、もうちょっとでも、早かったなら！」
「いや、まだ陽は沈まぬ。」メロスは胸の張り裂ける思いで、赤く大きい夕陽ばかりを見つめていた。走るより他は無い。
「やめて下さい。走るのは、やめて下さい。いまはご自分のお命が大事です。あの方は、あなたを信じて居りました。刑場に引き出されても、平気でいました。王様が、さんざんあの方をからかっても、メロスは来ます、とだけ答え、強い信念を持ちつづけている様子でございました。」
「それだから、走るのだ。信じられているから走るのだ。間に合う、間に合わぬは問題でないのだ。人の命も問題でないのだ。私は、なんだか、もっと恐ろしく大きいものの為に走っているのだ。ついて来い！　フィロストラトス。」
「ああ、あなたは気が狂ったか。それでは、うんと走るがいい。ひょっとしたら、間に合わぬものでもない。走るがいい。」
　言うにや及ぶ。まだ陽は沈まぬ。最後の死力を尽して、メロスは走った。メロスの頭は、からっぽだ。何一つ考えていない。ただ、わけのわからぬ大きな力にひきずられて走った。陽は、ゆらゆら地平線に没し、まさに最後の一片の残光も、消えようとした時、メロスは疾風の如く刑場に突入した。間に合った。
「待て。その人を殺してはならぬ。メロスが帰って来た。約束のとおり、いま、帰って来た。」と大声で刑場の群衆にむかって叫んだつもりであったが、喉のどがつぶれて嗄しわがれた声が幽かすかに出たばかり、群衆は、ひとりとして彼の到着に気がつかない。すでに磔の柱が高々と立てられ、縄を打たれたセリヌンティウスは、徐々に釣り上げられてゆく。メロスはそれを目撃して最後の勇、先刻、濁流を泳いだように群衆を掻きわけ、掻きわけ、
「私だ、刑吏！　殺されるのは、私だ。メロスだ。彼を人質にした私は、ここにいる！」と、かすれた声で精一ぱいに叫びながら、ついに磔台に昇り、釣り上げられてゆく友の両足に、齧かじりついた。群衆は、どよめいた。あっぱれ。ゆるせ、と口々にわめいた。セリヌンティウスの縄は、ほどかれたのである。
「セリヌンティウス。」メロスは眼に涙を浮べて言った。「私を殴れ。ちから一ぱいに頬を殴れ。私は、途中で一度、悪い夢を見た。君が若もし私を殴ってくれなかったら、私は君と抱擁する資格さえ無いのだ。殴れ。」
　セリヌンティウスは、すべてを察した様子で首肯うなずき、刑場一ぱいに鳴り響くほど音高くメロスの右頬を殴った。殴ってから優しく微笑ほほえみ、
「メロス、私を殴れ。同じくらい音高く私の頬を殴れ。私はこの三日の間、たった一度だけ、ちらと君を疑った。生れて、はじめて君を疑った。君が私を殴ってくれなければ、私は君と抱擁できない。」
　メロスは腕に唸うなりをつけてセリヌンティウスの頬を殴った。
「ありがとう、友よ。」二人同時に言い、ひしと抱き合い、それから嬉し泣きにおいおい声を放って泣いた。
　群衆の中からも、歔欷きょきの声が聞えた。暴君ディオニスは、群衆の背後から二人の様を、まじまじと見つめていたが、やがて静かに二人に近づき、顔をあからめて、こう言った。
「おまえらの望みは叶かなったぞ。おまえらは、わしの心に勝ったのだ。信実とは、決して空虚な妄想ではなかった。どうか、わしをも仲間に入れてくれまいか。どうか、わしの願いを聞き入れて、おまえらの仲間の一人にしてほしい。」
　どっと群衆の間に、歓声が起った。
「万歳、王様万歳。」
　ひとりの少女が、緋ひのマントをメロスに捧げた。メロスは、まごついた。佳き友は、気をきかせて教えてやった。
「メロス、君は、まっぱだかじゃないか。早くそのマントを着るがいい。この可愛い娘さんは、メロスの裸体を、皆に見られるのが、たまらなく口惜しいのだ。」
　勇者は、ひどく赤面した。
（古伝説と、シルレルの詩から。）




底本：「太宰治全集3」ちくま文庫、筑摩書房
　　　1988（昭和63）年10月25日初版発行
　　　1998（平成10）年6月15日第2刷
底本の親本：「筑摩全集類聚版太宰治全集」筑摩書房
　　　1975（昭和50）年6月～1976（昭和51）年6月
入力：金川一之
校正：高橋美奈子
2000年12月4日公開
2011年1月17日修正
青空文庫作成ファイル：
このファイルは、インターネットの図書館、青空文庫（http://www.aozora.gr.jp/）で作られました。入力、校正、制作にあたったのは、ボランティアの皆さんです。



●表記について
このファイルは W3C 勧告 XHTML1.1 にそった形式で作成されています。

●図書カード
""")]
    [InlineData("""

Page semi-protected
The Buddha
From Wikipedia, the free encyclopedia
(Redirected from Buddha)
Jump to navigationJump to search
"Buddha" and "Gautama" redirect here. For other uses, see Buddha (disambiguation) and Gautama (disambiguation).
The Buddha
Buddha in Sarnath Museum (Dhammajak Mutra).jpg
Statue of the Buddha, preaching his first sermon at Sarnath. Gupta period, ca. 475 CE. Archaeological Museum Sarnath (B(b) 181).[a]
Personal
Born	Siddhartha Gautama
c. 563 BCE or 480 BCE
Lumbini, Shakya Republic (according to Buddhist tradition)[b]
Died	c. 483 BCE or 400 BCE (aged 80)[1][2][3][c]
Kushinagar, Malla Republic (according to Buddhist tradition)[d]
Resting place	Cremated; ashes divided among followers
Spouse	Yashodhara
Children	
Rāhula
Parents	
Śuddhodana (father)
Maya Devi (mother)
Known for	Founding Buddhism
Other names	Shakyamuni ("Sage of the Shakyas")
Senior posting
Predecessor	Kassapa Buddha
Successor	Maitreya
Sanskrit name
Sanskrit	Siddhārtha Gautama
Pali name
Pali	Siddhattha Gotama
Part of a series on
Buddhism
Dharma Wheel.svg
History
DharmaConcepts
Buddhist texts
Practices
Nirvāṇa
Traditions
Buddhism by country
GlossaryIndexOutline
icon Religion portal
vte
Siddhartha Gautama (5th cent. BCE),[c] most commonly referred to as the Buddha,[e][f] was a South Asian renunciate[4] who founded Buddhism.

According to Buddhist tradition, he was born in Lumbini in what is now Nepal,[b] to royal parents of the Shakya clan, but renounced his home life to live "the holy life" as a homeless wanderer.[4][5][g] Leading a life of begging, asceticism, and meditation, he attained enlightenment at Bodh Gaya in what is now India. The Buddha thereafter travelled through the middle Gangetic Plain, teaching a Middle Way between sensual indulgence and severe asceticism,[6] inspiring a sangha ("community")[h] of like-minded śramaṇas. His teachings are summarized in the Noble Eightfold Path, a training of the mind that includes ethical training and meditative practices such as sense restraint, kindness toward others, mindfulness, and jhana/dhyana. He died in Kushinagar, attaining paranirvana.[d] The Buddha has since been venerated by numerous religions and communities across Asia.

His teachings were compiled by the Buddhist community in the Vinaya, the rules and procedures that govern the sangha, and the Sutta Piṭaka, a compilation of teachings based on his discourses. These were passed down in Middle Indo-Aryan dialects through an oral tradition.[7][8] Later generations composed additional texts, such as systematic treatises known as Abhidharma, biographies of the Buddha, collections of stories about his past lives known as Jataka tales, and additional discourses, i.e. the Mahayana sutras.[9][10]


Contents
1	Etymology, names and titles
1.1	Siddhārtha Gautama and Buddha Shakyamuni
1.2	Tathāgata
1.3	Common epithets
2	Sources
2.1	Historical sources
2.1.1	Pali suttas
2.1.2	Pillar and rock inscriptions
2.1.3	Oldest surviving manuscripts
2.2	Biographical sources
3	Historical person
3.1	Understanding the historical person
3.2	Dating
3.3	Historical context
3.3.1	Shakyas
3.3.2	Shramanas
3.3.3	Urban environment and egalitarism
4	Semi-legendary biography
4.1	Nature of traditional depictions
4.2	Previous lives
4.3	Birth and early life
4.4	Renunciation
4.5	Ascetic life and awakening
4.6	First sermon and formation of the saṅgha
4.7	Travels and growth of the saṅgha
4.8	Formation of the bhikkhunī order
4.9	Later years
4.10	Last days and parinirvana
4.11	Posthumous events
5	Teachings and views
5.1	Core teachings
5.1.1	Samsara
5.1.2	The six sense bases and the five aggregates
5.1.3	Dependent Origination
5.1.3.1	Anatta
5.1.4	The path to liberation
5.1.5	Jain and Brahmanical influences
5.1.6	Scholarly views on the earliest teachings
5.2	Homeless life
5.3	Society
5.3.1	Critique of Brahmanism
5.3.2	Socio-political teachings
5.3.3	Worldly happiness
6	Physical characteristics
7	In other religions
7.1	Hinduism
7.2	Islam
7.3	Christianity
7.4	Other religions
8	Artistic depictions
8.1	Gallery showing different Buddha styles
8.2	In other media
9	See also
10	References
10.1	Notes
10.2	Citations
10.3	Sources
11	Further reading
12	External links
Etymology, names and titles

The Buddha, Tapa Shotor monastery in Hadda, Afghanistan, 2nd century CE
Siddhārtha Gautama and Buddha Shakyamuni
According to Donald Lopez Jr., "... he tended to be known as either Buddha or Sakyamuni in China, Korea, Japan, and Tibet, and as either Gotama Buddha or Samana Gotama (“the ascetic Gotama”) in Sri Lanka and Southeast Asia."[11]

His family name was Siddhārtha Gautama (Pali: Siddhattha Gotama). "Siddhārtha" (Sanskrit; P. Siddhattha; T. Don grub; C. Xidaduo; J. Shiddatta/Shittatta; K. Siltalta) means "He Who Achieves His Goal."[12] The clan name of Gautama means "descendant of Gotama", "Gotama" meaning "one who has the most light,"[13] and comes from the fact that Kshatriya clans adopted the names of their house priests.[14][15]

While term "Buddha" is used in the Agamas and the Pali Canon, the oldest surviving written records of the term "Buddha" is from the middle of the 3rd century BCE, when several Edicts of Ashoka (reigned c. 269–232 BCE) mention the Buddha and Buddhism.[16][17] Ashoka's Lumbini pillar inscription commemorates the Emperor's pilgrimage to Lumbini as the Buddha's birthplace, calling him the Buddha Shakyamuni (Brahmi script: 𑀩𑀼𑀥 𑀲𑀓𑁆𑀬𑀫𑀼𑀦𑀻 Bu-dha Sa-kya-mu-nī, "Buddha, Sage of the Shakyas").[18]

Buddha, "Awakened One" or "Enlightened One,"[19][20][f] is the masculine form of budh (बुध् ), "to wake, be awake, observe, heed, attend, learn, become aware of, to know, be conscious again,"[21] "to awaken"[22][23] ""to open up" (as does a flower),"[23] "one who has awakened from the deep sleep of ignorance and opened his consciousness to encompass all objects of knowledge."[23] It is not a personal name, but a title for those who have attained bodhi (awakening, enlightenment).[22] Buddhi, the power to "form and retain concepts, reason, discern, judge, comprehend, understand,"[21] is the faculty which discerns truth (satya) from falsehood.

Shakyamuni (Sanskrit: [ɕaːkjɐmʊnɪ bʊddʱɐ]) means "Sage of the Shakyas."[24]

Tathāgata
Tathāgata (Pali; Pali: [tɐˈtʰaːɡɐtɐ]) is a term the Buddha commonly uses when referring to himself or other Buddhas in the Pāli Canon.[25] The exact meaning of the term is unknown, but is often thought to mean either "one who has thus gone" (tathā-gata), "one who has thus come" (tathā-āgata), or sometimes "one who has thus not gone" (tathā-agata). This is interpreted as signifying that the Tathāgata is beyond all coming and going – beyond all transitory phenomena. [26] A tathāgata is "immeasurable", "inscrutable", "hard to fathom", and "not apprehended."[27]

Common epithets
A common list of epithets are commonly seen together in the canonical texts, and depict some of his perfected qualities:[28]

Bhagavato (Bhagavan) – The Blessed one, one of the most used epithets, together with tathāgata[25]
Sammasambuddho – Perfectly self-awakened
Vijja-carana-sampano – Endowed with higher knowledge and ideal conduct.
Sugata – Well-gone or Well-spoken.
Lokavidu – Knower of the many worlds.
Anuttaro Purisa-damma-sarathi – Unexcelled trainer of untrained people.
Satthadeva-Manussanam – Teacher of gods and humans.
Araham – Worthy of homage. An Arahant is "one with taints destroyed, who has lived the holy life, done what had to be done, laid down the burden, reached the true goal, destroyed the fetters of being, and is completely liberated through final knowledge."
Jina – Conqueror. Although the term is more commonly used to name an individual who has attained liberation in the religion Jainism, it is also an alternative title for the Buddha.[29]
The Pali Canon also contains numerous other titles and epithets for the Buddha, including: All-seeing, All-transcending sage, Bull among men, The Caravan leader, Dispeller of darkness, The Eye, Foremost of charioteers, Foremost of those who can cross, King of the Dharma (Dharmaraja), Kinsman of the Sun, Helper of the World (Lokanatha), Lion (Siha), Lord of the Dhamma, Of excellent wisdom (Varapañña), Radiant One, Torchbearer of mankind, Unsurpassed doctor and surgeon, Victor in battle, and Wielder of power.[30] Another epithet, used at inscriptions throughout South and Southeast Asia, is Maha sramana, "great sramana" (ascetic, renunciate).

Sources
Historical sources
Pali suttas
Main article: Early Buddhist Texts
On the basis of philological evidence, Indologist and Pāli expert Oskar von Hinüber says that some of the Pāli suttas have retained very archaic place-names, syntax, and historical data from close to the Buddha's lifetime, including the Mahāparinibbāṇa Sutta which contains a detailed account of the Buddha's final days. Hinüber proposes a composition date of no later than 350–320 BCE for this text, which would allow for a "true historical memory" of the events approximately 60 years prior if the Short Chronology for the Buddha's lifetime is accepted (but he also points out that such a text was originally intended more as hagiography than as an exact historical record of events).[31][32]

John S. Strong sees certain biographical fragments in the canonical texts preserved in Pāli, as well as Chinese, Tibetan and Sanskrit as the earliest material. These include texts such as the "Discourse on the Noble Quest" (: Ariyapariyesanā-sutta) and its parallels in other languages.[33]

Pillar and rock inscriptions


Ashoka's Lumbini pillar inscription (c. 250 BCE), with the words "Bu-dhe" (𑀩𑀼𑀥𑁂, the Buddha) and "Sa-kya-mu-nī " (𑀲𑀓𑁆𑀬𑀫𑀼𑀦𑀻, "Sage of the Shakyas") in the Brahmi script.[34][35][36]

Inscription "The illumination of the Blessed Sakamuni" (Brahmi script: 𑀪𑀕𑀯𑀢𑁄 𑀲𑀓𑀫𑀼𑀦𑀺𑀦𑁄 𑀩𑁄𑀥𑁄, Bhagavato Sakamunino Bodho) on a relief showing the "empty" Illumination Throne of the Buddha in the early Mahabodhi Temple at Bodh Gaya. Bharhut, c. 100 BCE.[37][38][39]
No written records about Gautama were found from his lifetime or from the one or two centuries thereafter.[16][17][40] But from the middle of the 3rd century BCE, several Edicts of Ashoka (reigned c. 268 to 232 BCE) mention the Buddha and Buddhism.[16][17] Particularly, Ashoka's Lumbini pillar inscription commemorates the Emperor's pilgrimage to Lumbini as the Buddha's birthplace, calling him the Buddha Shakyamuni (Brahmi script: 𑀩𑀼𑀥 𑀲𑀓𑁆𑀬𑀫𑀼𑀦𑀻 Bu-dha Sa-kya-mu-nī, "Buddha, Sage of the Shakyas").[i][34][35] Another one of his edicts (Minor Rock Edict No. 3) mentions the titles of several Dhamma texts (in Buddhism, "dhamma" is another word for "dharma"),[41] establishing the existence of a written Buddhist tradition at least by the time of the Maurya era. These texts may be the precursor of the Pāli Canon.[42][43][j]

"Sakamuni" is also mentioned in the reliefs of Bharhut, dated to c. 100 BCE, in relation with his illumination and the Bodhi tree, with the inscription Bhagavato Sakamunino Bodho ("The illumination of the Blessed Sakamuni").[38][37]

Oldest surviving manuscripts
The oldest surviving Buddhist manuscripts are the Gandhāran Buddhist texts, found in Gandhara (corresponding to modern northwestern Pakistan and eastern Afghanistan) and written in Gāndhārī, they date from the first century BCE to the third century CE.[44]

Biographical sources
Early canonical sources include the Ariyapariyesana Sutta (MN 26), the Mahāparinibbāṇa Sutta (DN 16), the Mahāsaccaka-sutta (MN 36), the Mahapadana Sutta (DN 14), and the Achariyabhuta Sutta (MN 123), which include selective accounts that may be older, but are not full biographies. The Jātaka tales retell previous lives of Gautama as a bodhisattva, and the first collection of these can be dated among the earliest Buddhist texts.[45] The Mahāpadāna Sutta and Achariyabhuta Sutta both recount miraculous events surrounding Gautama's birth, such as the bodhisattva's descent from the Tuṣita Heaven into his mother's womb.

The sources which present a complete picture of the life of Siddhārtha Gautama are a variety of different, and sometimes conflicting, traditional biographies from a later date. These include the Buddhacarita, Lalitavistara Sūtra, Mahāvastu, and the Nidānakathā.[46] Of these, the Buddhacarita[47][48][49] is the earliest full biography, an epic poem written by the poet Aśvaghoṣa in the first century CE.[50] The Lalitavistara Sūtra is the next oldest biography, a Mahāyāna/Sarvāstivāda biography dating to the 3rd century CE.[51] The Mahāvastu from the Mahāsāṃghika Lokottaravāda tradition is another major biography, composed incrementally until perhaps the 4th century CE.[51] The Dharmaguptaka biography of the Buddha is the most exhaustive, and is entitled the Abhiniṣkramaṇa Sūtra,[52] and various Chinese translations of this date between the 3rd and 6th century CE. The Nidānakathā is from the Theravada tradition in Sri Lanka and was composed in the 5th century by Buddhaghoṣa.[53]

Historical person
Understanding the historical person
Scholars are hesitant to make claims about the historical facts of the Buddha's life. Most of them accept that the Buddha lived, taught, and founded a monastic order during the Mahajanapada, and during the reign of Bimbisara, the ruler of the Magadha empire; and died during the early years of the reign of Ajatashatru, who was the successor of Bimbisara, thus making him a younger contemporary of Mahavira, the Jain tirthankara.[54][55]

There is less consensus on the veracity of many details contained in traditional biographies,[56][57] as "Buddhist scholars [...] have mostly given up trying to understand the historical person."[58] The earliest versions of Buddhist biographical texts that we have already contain many supernatural, mythical or legendary elements. In the 19th century some scholars simply omitted these from their accounts of the life, so that "the image projected was of a Buddha who was a rational, socratic teacher—a great person perhaps, but a more or less ordinary human being". More recent scholars tend to see such demythologisers as remythologisers, "creating a Buddha that appealed to them, by eliding one that did not".[59]

Dating
The dates of Gautama's birth and death are uncertain. Within the Eastern Buddhist tradition of China, Vietnam, Korea and Japan, the traditional date for the death of the Buddha was 949 BCE.[1] According to the Ka-tan system of time calculation in the Kalachakra tradition, Buddha is believed to have died about 833 BCE.[60]

Buddhist texts present two chronologies which have been used to date the lifetime of the Buddha.[61] The "long chronology," from Sri Lankese chronicles, states that the Buddha was born 298 years before the coronation of Asoka, and died 218 years before his coronation. According to these chronicles Asoka was crowned in 326 BCE, which gives the dates of 624 and 544 BCE for the Buddha, which are the accepted dates in Sri Lanka and South-East Asia.[61] However, most scholars who accept the long chronology date Asoka's coronation to 268 or 267 BCE, based on Greek evidence, thus dating the Buddha at 566 and ca. 486.[61]

Indian sources, and their Chinese and Tibetan translations, contain a "short chronology," which place the Buddha's birth at 180 years before Asoka's coronation, and his death 100 years before Asoka's coronation. Following the Greek sources of Asoka's coronation, this dates the Buddha at 448 and 368 BCE.[61]

Most historians in the early 20th century dated his lifetime as c. 563 BCE to 483 BCE.[1][62] More recently his death is dated later, between 411 and 400 BCE, while at a symposium on this question held in 1988,[63][64][65] the majority of those who presented definite opinions gave dates within 20 years either side of 400 BCE for the Buddha's death.[1][66][c] These alternative chronologies, however, have not been accepted by all historians.[71][72][k]

The dating of Bimbisara and Ajatashatru also depends on the long or short chronology. In the long chrononology, Bimbisara reigned c. 558 – c. 492 BCE, and died 492 BCE,[77][78] while Ajatashatru reigned c. 492 – c. 460 BCE.[79] In the short chronology Bimbisara reigned c. 400 BCE,[80][l] while Ajatashatru died between c. 380 BCE and 330 BCE.[80])

Historical context

Ancient kingdoms and cities of India during the time of the Buddha (c.  500 BCE)
Shakyas
According to the Buddhist tradition, Shakyamuni Buddha was a Sakya, a sub-Himalayan ethnicity and clan of north-eastern region of the Indian subcontinent.[b][m] The Shakya community was on the periphery, both geographically and culturally, of the eastern Indian subcontinent in the 5th century BCE.[81] The community, though describable as a small republic, was probably an oligarchy, with his father as the elected chieftain or oligarch.[81] The Shakyas were widely considered to be non-Vedic (and, hence impure) in Brahminic texts; their origins remain speculative and debated.[82] Bronkhorst terms this culture, which grew alongside Aryavarta without being affected by the flourish of Brahminism, as Greater Magadha.[83]

The Buddha's tribe of origin, the Shakyas, seems to have had non-Vedic religious practices which persist in Buddhism, such as the veneration of trees and sacred groves, and the worship of tree spirits (yakkhas) and serpent beings (nagas). They also seem to have built burial mounds called stupas.[82] Tree veneration remains important in Buddhism today, particularly in the practice of venerating Bodhi trees. Likewise, yakkas and nagas have remained important figures in Buddhist religious practices and mythology.[82]

Shramanas
The Buddha's lifetime coincided with the flourishing of influential śramaṇa schools of thought like Ājīvika, Cārvāka, Jainism, and Ajñana.[84] The Brahmajala Sutta records sixty-two such schools of thought. In this context, a śramaṇa refers to one who labours, toils or exerts themselves (for some higher or religious purpose). It was also the age of influential thinkers like Mahavira,[85] Pūraṇa Kassapa, Makkhali Gosāla, Ajita Kesakambalī, Pakudha Kaccāyana, and Sañjaya Belaṭṭhaputta, as recorded in Samaññaphala Sutta, with whose viewpoints the Buddha must have been acquainted.[86][87][n] Śāriputra and Moggallāna, two of the foremost disciples of the Buddha, were formerly the foremost disciples of Sañjaya Belaṭṭhaputta, the sceptic;[89] and the Pāli canon frequently depicts Buddha engaging in debate with the adherents of rival schools of thought. There is also philological evidence to suggest that the two masters, Alara Kalama and Uddaka Rāmaputta, were indeed historical figures and they most probably taught Buddha two different forms of meditative techniques.[90] Thus, Buddha was just one of the many śramaṇa philosophers of that time.[91] In an era where holiness of person was judged by their level of asceticism,[92] Buddha was a reformist within the śramaṇa movement, rather than a reactionary against Vedic Brahminism.[93]

Coningham and Young note that both Jains and Buddhists used stupas, while tree shines can be found in both Buddhism and Hinduism.[94]

Urban environment and egalitarism
See also: Greater Magadha
The rise of Buddhism coincided with the Second Urbanisation, in which the Ganges Basin was settled and cities grew, in which egalitarism prevailed. According to Thapar, the Buddha's teachings were "also a response to the historical changes of the time, among which were the emergence of the state and the growth of urban centres."[95] While the Buddhist mendicants renounced society, they lived close to the villages and cities, depending for alms-givings on lay supporters.[95]

According to Dyson, the Ganges basin was settled from the north-west and the south-east, as well as from within, "[coming] together in what is now Bihar (the location of Pataliputra )."[96] The Ganges basin was densely forested, and the population grew when new areas were deforestated and cultivated.[96] The society of the middle Ganges basin lay on "the outer fringe of Aryan cultural influence,"[97] and differed significantly from the Aryan society of the western Ganges basin.[98][99] According to Stein and Burton, "[t]he gods of the brahmanical sacrificial cult were not rejected so much as ignored by Buddhists and their contemporaries."[98] Jainism and Buddhism opposed the social stratification of Brahmanism, and their egalitarism prevailed in the cities of the middle Ganges basin.[97] This "allowed Jains and Buddhists to engage in trade more easily than Brahmans, who were forced to follow strict caste prohibitions."[100]

Semi-legendary biography

One of the earliest anthropomorphic representations of the Buddha, here surrounded by Brahma (left) and Śakra (right). Bimaran Casket, mid-1st century CE, British Museum.[101][102]
Nature of traditional depictions

Māyā miraculously giving birth to Siddhārtha. Sanskrit, palm-leaf manuscript. Nālandā, Bihar, India. Pāla period
In the earliest Buddhist texts, the nikāyas and āgamas, the Buddha is not depicted as possessing omniscience (sabbaññu)[103] nor is he depicted as being an eternal transcendent (lokottara) being. According to Bhikkhu Analayo, ideas of the Buddha's omniscience (along with an increasing tendency to deify him and his biography) are found only later, in the Mahayana sutras and later Pali commentaries or texts such as the Mahāvastu.[103] In the Sandaka Sutta, the Buddha's disciple Ananda outlines an argument against the claims of teachers who say they are all knowing [104] while in the Tevijjavacchagotta Sutta the Buddha himself states that he has never made a claim to being omniscient, instead he claimed to have the "higher knowledges" (abhijñā).[105] The earliest biographical material from the Pali Nikayas focuses on the Buddha's life as a śramaṇa, his search for enlightenment under various teachers such as Alara Kalama and his forty-five-year career as a teacher.[106]

Traditional biographies of Gautama often include numerous miracles, omens, and supernatural events. The character of the Buddha in these traditional biographies is often that of a fully transcendent (Skt. lokottara) and perfected being who is unencumbered by the mundane world. In the Mahāvastu, over the course of many lives, Gautama is said to have developed supramundane abilities including: a painless birth conceived without intercourse; no need for sleep, food, medicine, or bathing, although engaging in such "in conformity with the world"; omniscience, and the ability to "suppress karma".[107] As noted by Andrew Skilton, the Buddha was often described as being superhuman, including descriptions of him having the 32 major and 80 minor marks of a "great man", and the idea that the Buddha could live for as long as an aeon if he wished (see DN 16).[108]

The ancient Indians were generally unconcerned with chronologies, being more focused on philosophy. Buddhist texts reflect this tendency, providing a clearer picture of what Gautama may have taught than of the dates of the events in his life. These texts contain descriptions of the culture and daily life of ancient India which can be corroborated from the Jain scriptures, and make the Buddha's time the earliest period in Indian history for which significant accounts exist.[109] British author Karen Armstrong writes that although there is very little information that can be considered historically sound, we can be reasonably confident that Siddhārtha Gautama did exist as a historical figure.[110] Michael Carrithers goes a bit further by stating that the most general outline of "birth, maturity, renunciation, search, awakening and liberation, teaching, death" must be true.[111]

Previous lives

The legendary Jataka collections depict the Buddha-to-be in a previous life prostrating before the past Buddha Dipankara, making a resolve to be a Buddha, and receiving a prediction of future Buddhahood.
Legendary biographies like the Pali Buddhavaṃsa and the Sanskrit Jātakamālā depict the Buddha's (referred to as "bodhisattva" before his awakening) career as spanning hundreds of lifetimes before his last birth as Gautama. Many stories of these previous lives are depicted in the Jatakas.[112] The format of a Jataka typically begins by telling a story in the present which is then explained by a story of someone's previous life.[113]

Besides imbuing the pre-Buddhist past with a deep karmic history, the Jatakas also serve to explain the bodhisattva's (the Buddha-to-be) path to Buddhahood.[114] In biographies like the Buddhavaṃsa, this path is described as long and arduous, taking "four incalculable ages" (asamkheyyas).[115]

In these legendary biographies, the bodhisattva goes through many different births (animal and human), is inspired by his meeting of past Buddhas, and then makes a series of resolves or vows (pranidhana) to become a Buddha himself. Then he begins to receive predictions by past Buddhas.[116] One of the most popular of these stories is his meeting with Dipankara Buddha, who gives the bodhisattva a prediction of future Buddhahood.[117]

Another theme found in the Pali Jataka Commentary (Jātakaṭṭhakathā) and the Sanskrit Jātakamālā is how the Buddha-to-be had to practice several "perfections" (pāramitā) to reach Buddhahood.[118] The Jatakas also sometimes depict negative actions done in previous lives by the bodhisattva, which explain difficulties he experienced in his final life as Gautama.[119]

Birth and early life

Map showing Lumbini and other major Buddhist sites in India. Lumbini (present-day Nepal), is the birthplace of the Buddha,[120][b] and is a holy place also for many non-Buddhists.[121]

The Lumbini pillar contains an inscription stating that this is the Buddha's birthplace
According to the Buddhist tradition, Gautama was born in Lumbini,[120][122] now in modern-day Nepal,[o] and raised in Kapilavastu.[123]}}[p] The exact site of ancient Kapilavastu is unknown.[125] It may have been either Piprahwa, Uttar Pradesh, in present-day India,[126] or Tilaurakot, in present-day Nepal.[127] Both places belonged to the Sakya territory, and are located only 24 kilometres (15 mi) apart.[127][b]

In the mid-3rd century BCE the Emperor Ashoka determined that Lumbini was Gautama's birthplace and thus installed a pillar there with the inscription: "...this is where the Buddha, sage of the Śākyas (Śākyamuni), was born."[128]

According to later biographies such as the Mahavastu and the Lalitavistara, his mother, Maya (Māyādevī), Suddhodana's wife, was a princess from Devdaha, the ancient capital of the Koliya Kingdom (what is now the Rupandehi District of Nepal). Legend has it that, on the night Siddhartha was conceived, Queen Maya dreamt that a white elephant with six white tusks entered her right side,[129][130] and ten months later[131] Siddhartha was born. As was the Shakya tradition, when his mother Queen Maya became pregnant, she left Kapilavastu for her father's kingdom to give birth. However, her son is said to have been born on the way, at Lumbini, in a garden beneath a sal tree. The earliest Buddhist sources state that the Buddha was born to an aristocratic Kshatriya (Pali: khattiya) family called Gotama (Sanskrit: Gautama), who were part of the Shakyas, a tribe of rice-farmers living near the modern border of India and Nepal.[132][124][133][q] His father Śuddhodana was "an elected chief of the Shakya clan",[135] whose capital was Kapilavastu, and who were later annexed by the growing Kingdom of Kosala during the Buddha's lifetime. Gautama was his family name.

The early Buddhist texts contain very little information about the birth and youth of Gotama Buddha.[136][137] Later biographies developed a dramatic narrative about the life of the young Gotama as a prince and his existential troubles.[138] They also depict his father Śuddhodana as a hereditary monarch of the Suryavansha (Solar dynasty) of Ikṣvāku (Pāli: Okkāka). This is unlikely however, as many scholars think that Śuddhodana was merely a Shakya aristocrat (khattiya), and that the Shakya republic was not a hereditary monarchy.[139][140][141] Indeed, the more egalitarian gaṇasaṅgha form of government, as a political alternative to Indian monarchies, may have influenced the development of the śramanic Jain and Buddhist sanghas,[h] where monarchies tended toward Vedic Brahmanism.[142]

The day of the Buddha's birth is widely celebrated in Theravada countries as Vesak.[143] Buddha's Birthday is called Buddha Purnima in Nepal, Bangladesh, and India as he is believed to have been born on a full moon day.

According to later biographical legends, during the birth celebrations, the hermit seer Asita journeyed from his mountain abode, analyzed the child for the "32 marks of a great man" and then announced that he would either become a great king (chakravartin) or a great religious leader.[144][145] Suddhodana held a naming ceremony on the fifth day and invited eight Brahmin scholars to read the future. All gave similar predictions.[144] Kondañña, the youngest, and later to be the first arhat other than the Buddha, was reputed to be the only one who unequivocally predicted that Siddhartha would become a Buddha.[146]

Early texts suggest that Gautama was not familiar with the dominant religious teachings of his time until he left on his religious quest, which is said to have been motivated by existential concern for the human condition.[147] According to the early Buddhist Texts of several schools, and numerous post-canonical accounts, Gotama had a wife, Yasodhara, and a son, named Rāhula.[148] Besides this, the Buddha in the early texts reports that "'I lived a spoilt, a very spoilt life, monks (in my parents' home)."[149]

The legendary biographies like the Lalitavistara also tell stories of young Gotama's great martial skill, which was put to the test in various contests against other Shakyan youths.[150]

Renunciation
See also: Great Renunciation

The "Great Departure" of Siddhartha Gautama, surrounded by a halo, he is accompanied by numerous guards and devata who have come to pay homage; Gandhara, Kushan period
While the earliest sources merely depict Gotama seeking a higher spiritual goal and becoming an ascetic or śramaṇa after being disillusioned with lay life, the later legendary biographies tell a more elaborate dramatic story about how he became a mendicant.[138][151]

The earliest accounts of the Buddha's spiritual quest is found in texts such as the Pali Ariyapariyesanā-sutta ("The discourse on the noble quest," MN 26) and its Chinese parallel at MĀ 204.[152] These texts report that what led to Gautama's renunciation was the thought that his life was subject to old age, disease and death and that there might be something better (i.e. liberation, nirvana).[153] The early texts also depict the Buddha's explanation for becoming a sramana as follows: "The household life, this place of impurity, is narrow – the samana life is the free open air. It is not easy for a householder to lead the perfected, utterly pure and perfect holy life."[154] MN 26, MĀ 204, the Dharmaguptaka Vinaya and the Mahāvastu all agree that his mother and father opposed his decision and "wept with tearful faces" when he decided to leave.[155][156]


Prince Siddhartha shaves his hair and becomes a śramaṇa. Borobudur, 8th century
Legendary biographies also tell the story of how Gautama left his palace to see the outside world for the first time and how he was shocked by his encounter with human suffering.[157][158] These depict Gautama's father as shielding him from religious teachings and from knowledge of human suffering, so that he would become a great king instead of a great religious leader.[159] In the Nidanakatha (5th century CE), Gautama is said to have seen an old man. When his charioteer Chandaka explained to him that all people grew old, the prince went on further trips beyond the palace. On these he encountered a diseased man, a decaying corpse, and an ascetic that inspired him.[160][161][162] This story of the "four sights" seems to be adapted from an earlier account in the Digha Nikaya (DN 14.2) which instead depicts the young life of a previous Buddha, Vipassi.[162]

The legendary biographies depict Gautama's departure from his palace as follows. Shortly after seeing the four sights, Gautama woke up at night and saw his female servants lying in unattractive, corpse-like poses, which shocked him.[163] Therefore, he discovered what he would later understand more deeply during his enlightenment: suffering and the end of suffering.[164] Moved by all the things he had experienced, he decided to leave the palace in the middle of the night against the will of his father, to live the life of a wandering ascetic.[160] Accompanied by Chandaka and riding his horse Kanthaka, Gautama leaves the palace, leaving behind his son Rahula and Yaśodhara.[165] He travelled to the river Anomiya, and cut off his hair. Leaving his servant and horse behind, he journeyed into the woods and changed into monk's robes there,[166] though in some other versions of the story, he received the robes from a Brahma deity at Anomiya.[167]

According to the legendary biographies, when the ascetic Gautama first went to Rajagaha (present-day Rajgir) to beg for alms in the streets, King Bimbisara of Magadha learned of his quest, and offered him a share of his kingdom. Gautama rejected the offer but promised to visit his kingdom first, upon attaining enlightenment.[168][169]

Ascetic life and awakening

The gilded "Emaciated Buddha statue" in Wat Suthat in Bangkok representing the stage of his asceticism

The Mahabodhi Tree at the Sri Mahabodhi Temple in Bodh Gaya

The Enlightenment Throne of the Buddha at Bodh Gaya, as recreated by Emperor Ashoka in the 3rd century BCE.

Miracle of the Buddha walking on the River Nairañjanā. The Buddha is not visible (aniconism), only represented by a path on the water, and his empty throne bottom right.[170] Sanchi.
See also: Enlightenment in Buddhism
Main articles: Moksha and Nirvana (Buddhism)
Majjhima Nikaya 4 mentions that Gautama lived in "remote jungle thickets" during his years of spiritual striving and had to overcome the fear that he felt while living in the forests.[171] The Nikaya-texts also narrate that the ascetic Gautama practised under two teachers of yogic meditation.[172][173] According to the Ariyapariyesanā-sutta (MN 26) and its Chinese parallel at MĀ 204, after having mastered the teaching of Ārāḍa Kālāma (Pali: Alara Kalama), who taught a meditation attainment called "the sphere of nothingness", he was asked by Ārāḍa to become an equal leader of their spiritual community.[174][175] However, Gautama felt unsatisfied by the practice because it "does not lead to revulsion, to dispassion, to cessation, to calm, to knowledge, to awakening, to Nibbana", and moved on to become a student of Udraka Rāmaputra (Pali: Udaka Ramaputta).[176][177] With him, he achieved high levels of meditative consciousness (called "The Sphere of Neither Perception nor Non-Perception") and was again asked to join his teacher. But, once more, he was not satisfied for the same reasons as before, and moved on.[178]

According to some sutras, after leaving his meditation teachers, Gotama then practiced ascetic techniques.[179][r] The ascetic techniques described in the early texts include very minimal food intake, different forms of breath control, and forceful mind control. The texts report that he became so emaciated that his bones became visible through his skin.[181] The Mahāsaccaka-sutta and most of its parallels agree that after taking asceticism to its extremes, Gautama realized that this had not helped him attain nirvana, and that he needed to regain strength to pursue his goal.[182] One popular story tells of how he accepted milk and rice pudding from a village girl named Sujata.[183] His break with asceticism is said to have led his five companions to abandon him, since they believed that he had abandoned his search and become undisciplined. At this point, Gautama remembered a previous experience of dhyana he had as a child sitting under a tree while his father worked.[182] This memory leads him to understand that dhyana ("meditation") is the path to liberation, and the texts then depict the Buddha achieving all four dhyanas, followed by the "three higher knowledges" (tevijja),[s] culminating in complete insight into the Four Noble Truths, thereby attaining liberation from samsara, the endless cycle of rebirth.[185][186][187][188] [t]

According to the Dhammacakkappavattana Sutta (SN 56),[189] the Tathagata, the term Gautama uses most often to refer to himself, realized "the Middle Way"—a path of moderation away from the extremes of self-indulgence and self-mortification, or the Noble Eightfold Path.[189] In later centuries, Gautama became known as the Buddha or "Awakened One". The title indicates that unlike most people who are "asleep", a Buddha is understood as having "woken up" to the true nature of reality and sees the world 'as it is' (yatha-bhutam).[19] A Buddha has achieved liberation (vimutti), also called Nirvana, which is seen as the extinguishing of the "fires" of desire, hatred, and ignorance, that keep the cycle of suffering and rebirth going.[190]

Following his decision to leave his meditation teachers, MĀ 204 and other parallel early texts report that Gautama sat down with the determination not to get up until full awakening (sammā-sambodhi) had been reached; the Ariyapariyesanā-sutta does not mention "full awakening", but only that he attained nirvana.[191] This event was said to have occurred under a pipal tree—known as "the Bodhi tree"—in Bodh Gaya, Bihar.[192]

As reported by various texts from the Pali Canon, the Buddha sat for seven days under the bodhi tree "feeling the bliss of deliverance".[193] The Pali texts also report that he continued to meditate and contemplated various aspects of the Dharma while living by the River Nairañjanā, such as Dependent Origination, the Five Spiritual Faculties and Suffering.[194]

The legendary biographies like the Mahavastu, Nidanakatha and the Lalitavistara depict an attempt by Mara, the ruler of the desire realm, to prevent the Buddha's nirvana. He does so by sending his daughters to seduce the Buddha, by asserting his superiority and by assaulting him with armies of monsters.[195] However the Buddha is unfazed and calls on the earth (or in some versions of the legend, the earth goddess) as witness to his superiority by touching the ground before entering meditation.[196] Other miracles and magical events are also depicted.

First sermon and formation of the saṅgha

Dhamek Stupa in Sarnath, India, site of the first teaching of the Buddha in which he taught the Four Noble Truths to his first five disciples
According to MN 26, immediately after his awakening, the Buddha hesitated on whether or not he should teach the Dharma to others. He was concerned that humans were overpowered by ignorance, greed, and hatred that it would be difficult for them to recognise the path, which is "subtle, deep and hard to grasp". However, the god Brahmā Sahampati convinced him, arguing that at least some "with little dust in their eyes" will understand it. The Buddha relented and agreed to teach. According to Anālayo, the Chinese parallel to MN 26, MĀ 204, does not contain this story, but this event does appear in other parallel texts, such as in an Ekottarika-āgama discourse, in the Catusparisat-sūtra, and in the Lalitavistara.[191]

According to MN 26 and MĀ 204, after deciding to teach, the Buddha initially intended to visit his former teachers, Alara Kalama and Udaka Ramaputta, to teach them his insights, but they had already died, so he decided to visit his five former companions.[197] MN 26 and MĀ 204 both report that on his way to Vārānasī (Benares), he met another wanderer, called Ājīvika Upaka in MN 26. The Buddha proclaimed that he had achieved full awakening, but Upaka was not convinced and "took a different path".[198]

MN 26 and MĀ 204 continue with the Buddha reaching the Deer Park (Sarnath) (Mrigadāva, also called Rishipatana, "site where the ashes of the ascetics fell")[199] near Vārānasī, where he met the group of five ascetics and was able to convince them that he had indeed reached full awakening.[200] According to MĀ 204 (but not MN 26), as well as the Theravāda Vinaya, an Ekottarika-āgama text, the Dharmaguptaka Vinaya, the Mahīśāsaka Vinaya, and the Mahāvastu, the Buddha then taught them the "first sermon", also known as the "Benares sermon",[199] i.e. the teaching of "the noble eightfold path as the middle path aloof from the two extremes of sensual indulgence and self-mortification."[200] The Pali text reports that after the first sermon, the ascetic Koṇḍañña (Kaundinya) became the first arahant (liberated being) and the first Buddhist bhikkhu or monastic.[201] The Buddha then continued to teach the other ascetics and they formed the first saṅgha:[h] the company of Buddhist monks.

Various sources such as the Mahāvastu, the Mahākhandhaka of the Theravāda Vinaya and the Catusparisat-sūtra also mention that the Buddha taught them his second discourse, about the characteristic of "not-self" (Anātmalakṣaṇa Sūtra), at this time[202] or five days later.[199] After hearing this second sermon the four remaining ascetics also reached the status of arahant.[199]

The Theravāda Vinaya and the Catusparisat-sūtra also speak of the conversion of Yasa, a local guild master, and his friends and family, who were some of the first laypersons to be converted and to enter the Buddhist community.[203][199] The conversion of three brothers named Kassapa followed, who brought with them five hundred converts who had previously been "matted hair ascetics", and whose spiritual practice was related to fire sacrifices.[204][205] According to the Theravāda Vinaya, the Buddha then stopped at the Gayasisa hill near Gaya and delivered his third discourse, the Ādittapariyāya Sutta (The Discourse on Fire),[206] in which he taught that everything in the world is inflamed by passions and only those who follow the Eightfold path can be liberated.[199]

At the end of the rainy season, when the Buddha's community had grown to around sixty awakened monks, he instructed them to wander on their own, teach and ordain people into the community, for the "welfare and benefit" of the world.[207][199]

Travels and growth of the saṅgha

Kosala and Magadha in the post-Vedic period

The chief disciples of the Buddha, Mogallana (chief in psychic power) and Sariputta (chief in wisdom).

The remains of a section of Jetavana Monastery, just outside of ancient Savatthi, in Uttar Pradesh.
For the remaining 40 or 45 years of his life, the Buddha is said to have travelled in the Gangetic Plain, in what is now Uttar Pradesh, Bihar, and southern Nepal, teaching a diverse range of people: from nobles to servants, ascetics and householders, murderers such as Angulimala, and cannibals such as Alavaka.[208][151][209] According to Schumann, the Buddha's travels ranged from "Kosambi on the Yamuna (25 km south-west of Allahabad )", to Campa (40 km east of Bhagalpur)" and from "Kapilavatthu (95 km north-west of Gorakhpur) to Uruvela (south of Gaya)." This covers an area of 600 by 300 km.[210] His sangha[h] enjoyed the patronage of the kings of Kosala and Magadha and he thus spent a lot of time in their respective capitals, Savatthi and Rajagaha.[210]

Although the Buddha's language remains unknown, it is likely that he taught in one or more of a variety of closely related Middle Indo-Aryan dialects, of which Pali may be a standardisation.

The sangha wandered throughout the year, except during the four months of the Vassa rainy season when ascetics of all religions rarely travelled. One reason was that it was more difficult to do so without causing harm to flora and animal life.[211] The health of the ascetics might have been a concern as well.[212] At this time of year, the sangha would retreat to monasteries, public parks or forests, where people would come to them.

The first vassana was spent at Varanasi when the sangha was formed. According to the Pali texts, shortly after the formation of the sangha, the Buddha travelled to Rajagaha, capital of Magadha, and met with King Bimbisara, who gifted a bamboo grove park to the sangha.[213]

The Buddha's sangha continued to grow during his initial travels in north India. The early texts tell the story of how the Buddha's chief disciples, Sāriputta and Mahāmoggallāna, who were both students of the skeptic sramana Sañjaya Belaṭṭhiputta, were converted by Assaji.[214][215] They also tell of how the Buddha's son, Rahula, joined his father as a bhikkhu when the Buddha visited his old home, Kapilavastu.[216] Over time, other Shakyans joined the order as bhikkhus, such as Buddha's cousin Ananda, Anuruddha, Upali the barber, the Buddha's half-brother Nanda and Devadatta.[217][218] Meanwhile, the Buddha's father Suddhodana heard his son's teaching, converted to Buddhism and became a stream-enterer.

The early texts also mention an important lay disciple, the merchant Anāthapiṇḍika, who became a strong lay supporter of the Buddha early on. He is said to have gifted Jeta's grove (Jetavana) to the sangha at great expense (the Theravada Vinaya speaks of thousands of gold coins).[219][220]

Formation of the bhikkhunī order

Mahāprajāpatī, the first bhikkuni and Buddha's stepmother, ordains
The formation of a parallel order of female monastics (bhikkhunī) was another important part of the growth of the Buddha's community. As noted by Anālayo's comparative study of this topic, there are various versions of this event depicted in the different early Buddhist texts.[u]

According to all the major versions surveyed by Anālayo, Mahāprajāpatī Gautamī, Buddha's step-mother, is initially turned down by the Buddha after requesting ordination for her and some other women. Mahāprajāpatī and her followers then shave their hair, don robes and begin following the Buddha on his travels. The Buddha is eventually convinced by Ānanda to grant ordination to Mahāprajāpatī on her acceptance of eight conditions called gurudharmas which focus on the relationship between the new order of nuns and the monks.[222]

According to Anālayo, the only argument common to all the versions that Ananda uses to convince the Buddha is that women have the same ability to reach all stages of awakening.[223] Anālayo also notes that some modern scholars have questioned the authenticity of the eight gurudharmas in their present form due to various inconsistencies. He holds that the historicity of the current lists of eight is doubtful, but that they may have been based on earlier injunctions by the Buddha.[224][225] Anālayo also notes that various passages indicate that the reason for the Buddha's hesitation to ordain women was the danger that the life of a wandering sramana posed for women that were not under the protection of their male family members (such as dangers of sexual assault and abduction). Due to this, the gurudharma injunctions may have been a way to place "the newly founded order of nuns in a relationship to its male counterparts that resembles as much as possible the protection a laywoman could expect from her male relatives."[226]

Later years

Ajatashatru worships the Buddha, relief from the Bharhut Stupa at the Indian Museum, Kolkata
According to J.S. Strong, after the first 20 years of his teaching career, the Buddha seems to have slowly settled in Sravasti, the capital of the Kingdom of Kosala, spending most of his later years in this city.[220]

As the sangha[h] grew in size, the need for a standardized set of monastic rules arose and the Buddha seems to have developed a set of regulations for the sangha. These are preserved in various texts called "Pratimoksa" which were recited by the community every fortnight. The Pratimoksa includes general ethical precepts, as well as rules regarding the essentials of monastic life, such as bowls and robes.[227]

In his later years, the Buddha's fame grew and he was invited to important royal events, such as the inauguration of the new council hall of the Shakyans (as seen in MN 53) and the inauguration of a new palace by Prince Bodhi (as depicted in MN 85).[228] The early texts also speak of how during the Buddha's old age, the kingdom of Magadha was usurped by a new king, Ajatashatru, who overthrew his father Bimbisara. According to the Samaññaphala Sutta, the new king spoke with different ascetic teachers and eventually took refuge in the Buddha.[229] However, Jain sources also claim his allegiance, and it is likely he supported various religious groups, not just the Buddha's sangha exclusively.[230]

As the Buddha continued to travel and teach, he also came into contact with members of other śrāmana sects. There is evidence from the early texts that the Buddha encountered some of these figures and critiqued their doctrines. The Samaññaphala Sutta identifies six such sects.[231]

The early texts also depict the elderly Buddha as suffering from back pain. Several texts depict him delegating teachings to his chief disciples since his body now needed more rest.[232] However, the Buddha continued teaching well into his old age.

One of the most troubling events during the Buddha's old age was Devadatta's schism. Early sources speak of how the Buddha's cousin, Devadatta, attempted to take over leadership of the order and then left the sangha with several Buddhist monks and formed a rival sect. This sect is said to have also been supported by King Ajatashatru.[233][234] The Pali texts also depict Devadatta as plotting to kill the Buddha, but these plans all fail.[235] They also depict the Buddha as sending his two chief disciples (Sariputta and Moggallana) to this schismatic community in order to convince the monks who left with Devadatta to return.[236]

All the major early Buddhist Vinaya texts depict Devadatta as a divisive figure who attempted to split the Buddhist community, but they disagree on what issues he disagreed with the Buddha on. The Sthavira texts generally focus on "five points" which are seen as excessive ascetic practices, while the Mahāsaṅghika Vinaya speaks of a more comprehensive disagreement, which has Devadatta alter the discourses as well as monastic discipline.[237]

At around the same time of Devadatta's schism, there was also war between Ajatashatru's Kingdom of Magadha, and Kosala, led by an elderly king Pasenadi.[238] Ajatashatru seems to have been victorious, a turn of events the Buddha is reported to have regretted.[239]

Last days and parinirvana
Metal relief
This East Javanese relief depicts the Buddha in his final days, and Ānanda, his chief attendant.
The main narrative of the Buddha's last days, death and the events following his death is contained in the Mahaparinibbana Sutta (DN 16) and its various parallels in Sanskrit, Chinese, and Tibetan.[240] According to Anālayo, these include the Chinese Dirgha Agama 2, "Sanskrit fragments of the Mahaparinirvanasutra", and "three discourses preserved as individual translations in Chinese".[241]

The Mahaparinibbana sutta depicts the Buddha's last year as a time of war. It begins with Ajatashatru's decision to make war on the Vajjika League, leading him to send a minister to ask the Buddha for advice.[242] The Buddha responds by saying that the Vajjikas can be expected to prosper as long as they do seven things, and he then applies these seven principles to the Buddhist Sangha[h], showing that he is concerned about its future welfare. The Buddha says that the Sangha will prosper as long as they "hold regular and frequent assemblies, meet in harmony, do not change the rules of training, honour their superiors who were ordained before them, do not fall prey to worldly desires, remain devoted to forest hermitages, and preserve their personal mindfulness." He then gives further lists of important virtues to be upheld by the Sangha.[243]

The early texts also depict how the Buddha's two chief disciples, Sariputta and Moggallana, died just before the Buddha's death.[244] The Mahaparinibbana depicts the Buddha as experiencing illness during the last months of his life but initially recovering. It also depicts him as stating that he cannot promote anyone to be his successor. When Ānanda requested this, the Mahaparinibbana records his response as follows:[245]

Ananda, why does the Order of monks expect this of me? I have taught the Dhamma, making no distinction of "inner" and " outer": the Tathagata has no "teacher's fist" (in which certain truths are held back). If there is anyone who thinks: "I shall take charge of the Order", or "the Order is under my leadership", such a person would have to make arrangements about the Order. The Tathagata does not think in such terms. Why should the Tathagata make arrangements for the Order? I am now old, worn out … I have reached the term of life, I am turning eighty years of age. Just as an old cart is made to go by being held together with straps, so the Tathagata's body is kept going by being bandaged up … Therefore, Ananda, you should live as islands unto yourselves, being your own refuge, seeking no other refuge; with the Dhamma as an island, with the Dhamma as your refuge, seeking no other refuge… Those monks who in my time or afterwards live thus, seeking an island and a refuge in themselves and in the Dhamma and nowhere else, these zealous ones are truly my monks and will overcome the darkness (of rebirth).


Mahaparinirvana, Gandhara, 3rd or 4th century CE, gray schist

Mahaparinibbana scene, from the Ajanta caves
After travelling and teaching some more, the Buddha ate his last meal, which he had received as an offering from a blacksmith named Cunda. Falling violently ill, Buddha instructed his attendant Ānanda to convince Cunda that the meal eaten at his place had nothing to do with his death and that his meal would be a source of the greatest merit as it provided the last meal for a Buddha.[246]Bhikkhu Mettanando and Oskar von Hinüber argue that the Buddha died of mesenteric infarction, a symptom of old age, rather than food poisoning.[247][248]

The precise contents of the Buddha's final meal are not clear, due to variant scriptural traditions and ambiguity over the translation of certain significant terms. The Theravada tradition generally believes that the Buddha was offered some kind of pork, while the Mahayana tradition believes that the Buddha consumed some sort of truffle or other mushroom. These may reflect the different traditional views on Buddhist vegetarianism and the precepts for monks and nuns.[249] Modern scholars also disagree on this topic, arguing both for pig's flesh or some kind of plant or mushroom that pigs like to eat.[v] Whatever the case, none of the sources which mention the last meal attribute the Buddha's sickness to the meal itself.[250]

As per the Mahaparinibbana sutta, after the meal with Cunda, the Buddha and his companions continued travelling until he was too weak to continue and had to stop at Kushinagar, where Ānanda had a resting place prepared in a grove of Sala trees.[251][252] After announcing to the sangha at large that he would soon be passing away to final Nirvana, the Buddha ordained one last novice into the order personally, his name was Subhadda.[251] He then repeated his final instructions to the sangha, which was that the Dhamma and Vinaya was to be their teacher after his death. Then he asked if anyone had any doubts about the teaching, but nobody did.[253] The Buddha's final words are reported to have been: "All saṅkhāras decay. Strive for the goal with diligence (appamāda)" (Pali: 'vayadhammā saṅkhārā appamādena sampādethā').[254][255]

He then entered his final meditation and died, reaching what is known as parinirvana (final nirvana, the end of rebirth and suffering achieved after the death of the body). The Mahaparinibbana reports that in his final meditation he entered the four dhyanas consecutively, then the four immaterial attainments and finally the meditative dwelling known as nirodha-samāpatti, before returning to the fourth dhyana right at the moment of death.[256][252]


Buddha's cremation stupa, Kushinagar (Kushinara).

Piprahwa vase with relics of the Buddha. The inscription reads: ...salilanidhane Budhasa Bhagavate... (Brahmi script: ...𑀲𑀮𑀺𑀮𑀦𑀺𑀥𑀸𑀦𑁂 𑀩𑀼𑀥𑀲 𑀪𑀕𑀯𑀢𑁂...) "Relics of the Buddha Lord".
Posthumous events
See also: Śarīra and Relics associated with Buddha
According to the Mahaparinibbana sutta, the Mallians of Kushinagar spent the days following the Buddha's death honouring his body with flowers, music and scents.[257] The sangha[h] waited until the eminent elder Mahākassapa arrived to pay his respects before cremating the body.[258]

The Buddha's body was then cremated and the remains, including his bones, were kept as relics and they were distributed among various north Indian kingdoms like Magadha, Shakya and Koliya.[259] These relics were placed in monuments or mounds called stupas, a common funerary practice at the time. Centuries later they would be exhumed and enshrined by Ashoka into many new stupas around the Mauryan realm.[260][261] Many supernatural legends surround the history of alleged relics as they accompanied the spread of Buddhism and gave legitimacy to rulers.

According to various Buddhist sources, the First Buddhist Council was held shortly after the Buddha's death to collect, recite and memorize the teachings. Mahākassapa was chosen by the sangha to be the chairman of the council. However, the historicity of the traditional accounts of the first council is disputed by modern scholars.[262]

Teachings and views
See also: The Buddha and early Buddhism
Core teachings

Gandharan Buddhist birchbark scroll fragments
Main article: Early Buddhist Texts
A number of teachings and practices are deemed essential to Buddhism, including: the samyojana (fetters, chains or bounds), that is, the sankharas ("formations"), the kleshas (uwholesome mental states), including the three poisons, and the āsavas ("influx, canker"), that perpetuate saṃsāra, the repeated cycle of becoming; the six sense bases and the five aggregates, which describe the proces from sense contact to consciousness which lead to this bondage to saṃsāra; dependent origination, which describes this proces, and it's reversal, in detail; and the Middle Way, with the Four Noble Truths and the Noble Eightfold Path, which prescribes how this bondage can be reversed.

According to N. Ross Reat, the Theravada Pali texts and the Mahasamghika school's Śālistamba Sūtra share these basic teachings and practices.[263] Bhikkhu Analayo concludes that the Theravada Majjhima Nikaya and Sarvastivada Madhyama Agama contain mostly the same major doctrines.[264] Likewise, Richard Salomon has written that the doctrines found in the Gandharan Manuscripts are "consistent with non-Mahayana Buddhism, which survives today in the Theravada school of Sri Lanka and Southeast Asia, but which in ancient times was represented by eighteen separate schools."[265]

Samsara
All beings have deeply entrenched samyojana (fetters, chains or bounds), that is, the sankharas ("formations"), kleshas (uwholesome mental states), including the three poisons, and āsavas ("influx, canker"), that perpetuate saṃsāra, the repeated cycle of becoming and rebirth. According to the Pali suttas, the Buddha stated that "this saṃsāra is without discoverable beginning. A first point is not discerned of beings roaming and wandering on hindered by ignorance and fettered by craving."[266] In the Dutiyalokadhammasutta sutta (AN 8:6) the Buddha explains how "eight worldly winds" "keep the world turning around [...] Gain and loss, fame and disrepute, praise and blame, pleasure and pain." He then explains how the difference between a noble (arya) person and an uninstructed worldling is that a noble person reflects on and understands the impermanence of these conditions.[267]

This cycle of becoming is characterized by dukkha,[268] commonly referred to as "suffering," dukkha is more aptly rendered as "unsatisfactoriness" or "unease." It is the unsatisfactoriness and unease that comes with a life dictated by automatic responses and habituated selfishness,[269][270] and the unsatifacories of expecting enduring happiness from things which are impermanent, unstable and thus unreliable.[271] The ultimate noble goal should be liberation from this cycle.[272]

Samsara is dictated by karma, which is an impersonal natural law, similar to how certain seeds produce certain plants and fruits.[273].Karma is not the only cause for one's conditions, as the Buddha listed various physical and environmental causes alongside karma.[274] The Buddha's teaching of karma differed to that of the Jains and Brahmins, in that on his view, karma is primarily mental intention (as opposed to mainly physical action or ritual acts).[269] The Buddha is reported to have said "By karma I mean intention."[275] Richard Gombrich summarizes the Buddha's view of karma as follows: "all thoughts, words, and deeds derive their moral value, positive or negative, from the intention behind them."[276]

The six sense bases and the five aggregates
The āyatana (six sense bases) and the five skandhas (aggregates) describe how sensory contact leads to attachment and dukkha. The six sense bases are ear and sound, nose and odour, tongue and taste, body and touch, and mind and thoughts. Together they create the input feom which we create our world or reality, "the all." Thi process takes place through the five skandhas, "aggregates," "groups," "heaps," five groups of physical and mental processes,[277][278] anmely form (or material image, impression) (rupa), sensations (or feelings, received from form) (vedana), perceptions (samjna), mental activity or formations (sankhara), consciousness (vijnana).[279][280][281] They form part of other Buddhist teachings and lists, such as dependent origination, and explain how sensory input ultimately leads to bondage to samsara by the mental defilements.

Dependent Origination

Schist Buddha statue with the famed Ye Dharma Hetu dhāraṇī around the head, which was used as a common summary of Dependent Origination. It states: "Of those experiences that arise from a cause, The Tathāgata has said: 'this is their cause, And this is their cessation': This is what the Great Śramaṇa teaches."
In the early texts, the process of the arising of dukkha is explicated through the teaching of dependent origination,[269] which says that everything that exists or occurs is dependent on conditioning factors.[282] The most basic formulation of dependent origination is given in the early texts as: 'It being thus, this comes about' (Pali: evam sati idam hoti).[283] This can be taken to mean that certain phenomena only arise when there are other phenomena present, thus their arising is "dependent" on other phenomena.[283]

The philosopher Mark Siderits has outlined the basic idea of the Buddha's teaching of Dependent Origination of dukkha as follows:

given the existence of a fully functioning assemblage of psycho-physical elements (the parts that make up a sentient being), ignorance concerning the three characteristics of sentient existence—suffering, impermanence and non-self—will lead, in the course of normal interactions with the environment, to appropriation (the identification of certain elements as 'I' and 'mine'). This leads in turn to the formation of attachments, in the form of desire and aversion, and the strengthening of ignorance concerning the true nature of sentient existence. These ensure future rebirth, and thus future instances of old age, disease and death, in a potentially unending cycle.[269]

In numerous early texts, this basic principle is expanded with a list of phenomena that are said to be conditionally dependent,[284][w] as a result of later elaborations,[285][286][287][x] including Vedic cosmogenies as the basis for the first four links.[288][289][290] [291][292][293] According to Boisvert, nidana 3-10 correlate with the five skandhas.[294] According to Richard Gombrich, the twelve-fold list is a combination of two previous lists, the second list beginning with tanha, "thirst," the cause of suffering as described in the second noble truth".[295] According to Gombrich, the two lists were combined, resulting in contradictions in its reverse version.[295][y]

Anatta
The Buddha saw his analysis of dependent origination as a "Middle Way" between "eternalism" (sassatavada, the idea that some essence exists eternally) and "annihilationism" (ucchedavada, the idea that we go completely out of existence at death).[269][283] in this view, persons are just a causal series of impermanent psycho-physical elements,[269] which are anatta, without an independent or permanent self.[282] The Buddha instead held that all things in the world of our experience are transient and that there is no unchanging part to a person.[296] According to Richard Gombrich, the Buddha's position is simply that "everything is process".[297]

The Buddha's arguments against an unchanging self rely on the scheme of the five skandhas, as can be seen in the Pali Anattalakkhaṇa Sutta (and its parallels in Gandhari and Chinese).[298][299][300] In the early texts the Buddha teaches that all five aggregates, including consciousness (viññana, which was held by Brahmins to be eternal), arise due to dependent origination.[301] Since they are all impermanent, one cannot regard any of the psycho-physical processes as an unchanging self.[302][269] Even mental processes such as consciousness and will (cetana) are seen as being dependently originated and impermanent and thus do not qualify as a self (atman).[269]

The Buddha saw the belief in a self as arising from our grasping at and identifying with the various changing phenomena, as well as from ignorance about how things really are.[303] Furthermore, the Buddha held that we experience suffering because we hold on to erroneous self views.[304][305] As Rupert Gethin explains, for the Buddha, a person is

... a complex flow of physical and mental phenomena, but peel away these phenomena and look behind them and one just does not find a constant self that one can call one's own. My sense of self is both logically and emotionally just a label that I impose on these physical and mental phenomena in consequence of their connectedness.[306]

Due to this view (termed ), the Buddha's teaching was opposed to all soul theories of his time, including the Jain theory of a "jiva" ("life monad") and the Brahmanical theories of atman (Pali: atta) and purusha. All of these theories held that there was an eternal unchanging essence to a person, which was separate from all changing experiences,[307] and which transmigrated from life to life.[308][309][269] The Buddha's anti-essentialist view still includes an understanding of continuity through rebirth, it is just the rebirth of a process (karma), not an essence like the atman.[310]

The path to liberation

Gandharan sculpture depicting the Buddha in the full lotus seated meditation posture, 2nd–3rd century CE

Buddha Statues from Gal Vihara. The Early Buddhist texts also mention meditation practice while standing and lying down.
Main articles: Buddhist paths to liberation and Buddhist meditation
The Buddha taught a path (marga) of training to undo the samyojana, kleshas and āsavas and attain vimutti (liberation).[269][311] This path taught by the Buddha is depicted in the early texts (most famously in the Pali Dhammacakkappavattana Sutta and its numerous parallel texts) as a "Middle Way" between sensual indulgence on one hand and mortification of the body on the other.[312]

A common presentation of the core structure of Buddha's teaching found in the early texts is that of the Four Noble Truths,[313] which refers to the Noble Eightfold Path.[314][z] According to Gethin, another common summary of the path to awakening wisely used in the early texts is "abandoning the hindrances, practice of the four establishments of mindfulness and development of the awakening factors."[316]

According to Rupert Gethin, in the Nikayas and Agamas, the Buddha's path is mainly presented in a cumulative and gradual "step by step" process, such as that outlined in the Samaññaphala Sutta.[317][aa] Other early texts like the Upanisa sutta (SN 12.23), present the path as reversions of the process of Dependent Origination.[322][ab]

Bhāvanā, cultivation of wholesome states, is central to the Buddha's path. Common practices to this goal, which are shared by most of these early presentations of the path, include sila (ethical training), restraint of the senses (indriyasamvara), sati (mindfulness) and sampajañña (clear awareness), and the practice of dhyana, the cumulative development of wholesome states[318] leading to a "state of perfect equanimity and awareness (upekkhā-sati-parisuddhi)."[324] Dhyana is preceded and supported by various aspects of the path such as sense restraint[325] and mindfulness, which is elaborated in the satipatthana-scheme, as taught in the Pali Satipatthana Sutta and the sixteen elements of Anapanasati, as taught in the Anapanasati Sutta.[ac]

Jain and Brahmanical influences

The Bodhisattva meets with Alara Kalama, Borobudur relief.
In various texts, the Buddha is depicted as having studied under two named teachers, Āḷāra Kālāma and Uddaka Rāmaputta. According to Alexander Wynne, these were yogis who taught doctrines and practices similar to those in the Upanishads.[326] According to Johannes Bronkhorst, the "meditation without breath and reduced intake of food" which the Buddha practiced before his awakening are forms of asceticism which are similar to Jain practices.[327]

According to Richard Gombrich, the Buddha's teachings on Karma and Rebirth are a development of pre-Buddhist themes that can be found in Jain and Brahmanical sources, like the Brihadaranyaka Upanishad.[328] Likewise, samsara, the idea that we are trapped in cycles of rebirth and that we should seek liberation from them through non-harming (ahimsa) and spiritual practices, pre-dates the Buddha and was likely taught in early Jainism.[329] According to K.R. Norman, the Buddhist teaching of the three marks of existence[ad] may also reflect Upanishadic or other influences .[330] The Buddhist practice called Brahma-vihara may have also originated from a Brahmanic term;[331] but its usage may have been common in the sramana traditions.[332]

Scholarly views on the earliest teachings
Main article: Presectarian Buddhism

The Buddha on a coin of Kushan ruler Kanishka I, c. 130 CE.
One method to obtain information on the oldest core of Buddhism is to compare the oldest versions of the Pali Canon and other texts, such as the surviving portions of Sarvastivada, Mulasarvastivada, Mahisasaka, Dharmaguptaka,[333][334] and the Chinese Agamas.[335][336] The reliability of these sources, and the possibility of drawing out a core of oldest teachings, is a matter of dispute.[332][337][338][339] According to Lambert Schmithausen, there are three positions held by modern scholars of Buddhism with regard to the authenticity of the teachings contained in the Nikayas:[340]

"Stress on the fundamental homogeneity and substantial authenticity of at least a considerable part of the Nikayic materials."[ae]
"Scepticism with regard to the possibility of retrieving the doctrine of earliest Buddhism."[af]
"Cautious optimism in this respect."[ag]
Scholars such as Richard Gombrich, Akira Hirakawa, Alexander Wynne and A.K. Warder hold that these Early Buddhist Texts contain material that could possibly be traced to the Buddha.[339][345][153] Richard Gombrich argues that since the content of the earliest texts "presents such originality, intelligence, grandeur and—most relevantly—coherence...it is hard to see it as a composite work." Thus he concludes they are "the work of one genius."[346] Peter Harvey also agrees that "much" of the Pali Canon "must derive from his [the Buddha's] teachings."[347] Likewise, A. K. Warder has written that "there is no evidence to suggest that it [the shared teaching of the early schools] was formulated by anyone other than the Buddha and his immediate followers."[341] According to Alexander Wynne, "the internal evidence of the early Buddhist literature proves its historical authenticity."[348]

Other scholars of Buddhist studies have disagreed with the mostly positive view that the early Buddhist texts reflect the teachings of the historical Buddha, arguing that some teachings contained in the early texts are the authentic teachings of the Buddha, but not others. According to Tilmann Vetter, inconsistencies remain, and other methods must be applied to resolve those inconsistencies.[333][ah] According to Tilmann Vetter, the earliest core of the Buddhist teachings is the meditative practice of dhyāna,[351][ai] but "liberating insight" became an essential feature of the Buddhist tradition only at a later date. He posits that the Fourth Noble Truths, the Eightfold path and Dependent Origination, which are commonly seen as essential to Buddhism, are later formulations which form part of the explanatory framework of this "liberating insight".[353] Lambert Schmithausen similarly argues that the mention of the four noble truths as constituting "liberating insight", which is attained after mastering the four dhyānas, is a later addition.[349] Johannes Bronkhorst also argues that the four truths may not have been formulated in earliest Buddhism, and did not serve in earliest Buddhism as a description of "liberating insight".[354]

Edward Conze argued that the attempts of European scholars to reconstruct the original teachings of the Buddha were "all mere guesswork."[355]

Homeless life
The early Buddhist texts depict the Buddha as promoting the life of a homeless and celibate "sramana", or mendicant, as the ideal way of life for the practice of the path.[356] He taught that mendicants or "beggars" (bhikkhus) were supposed to give up all possessions and to own just a begging bowl and three robes.[357] As part of the Buddha's monastic discipline, they were also supposed to rely on the wider lay community for the basic necessities (mainly food, clothing, and lodging).[358]

The Buddha's teachings on monastic discipline were preserved in the various Vinaya collections of the different early schools.[357]

Buddhist monastics, which included both monks and nuns, were supposed to beg for their food, were not allowed to store up food or eat after noon and they were not allowed to use gold, silver or any valuables.[359][360]

Society
Critique of Brahmanism

Buddha meets a Brahmin, at the Indian Museum, Kolkata
According to Bronkhorst, "the bearers of [the Brahmanical] tradition, the Brahmins, did not occupy a dominant position in the area in which the Buddha preached his message."[99] Nevertheless, the Buddha was acquainted with Brahmanism, and in the early Buddhist Texts, the Buddha references Brahmanical devices. For example, in Samyutta Nikaya 111, Majjhima Nikaya 92 and Vinaya i 246 of the Pali Canon, the Buddha praises the Agnihotra as the foremost sacrifice and the Gayatri mantra as the foremost meter.[aj] In general, the Buddha critiques the Brahmanical religion and social system on certain key points.

The Brahmin caste held that the Vedas were eternal revealed (sruti) texts. The Buddha, on the other hand, did not accept that these texts had any divine authority or value.[362]

The Buddha also did not see the Brahmanical rites and practices as useful for spiritual advancement. For example, in the Udāna, the Buddha points out that ritual bathing does not lead to purity, only "truth and morality" lead to purity.[ak] He especially critiqued animal sacrifice as taught in Vedas.[362] The Buddha contrasted his teachings, which were taught openly to all people, with that of the Brahmins', who kept their mantras secret.[al]

The Buddha also critiqued the Brahmins' claims of superior birth and the idea that different castes and bloodlines were inherently pure or impure, noble or ignoble.[362]

In the Vasettha sutta the Buddha argues that the main difference among humans is not birth but their actions and occupations.[364] According to the Buddha, one is a "Brahmin" (i.e. divine, like Brahma) only to the extent that one has cultivated virtue.[am] Because of this the early texts report that he proclaimed: "Not by birth one is a Brahman, not by birth one is a non-Brahman; - by moral action one is a Brahman"[362]

The Aggañña Sutta explains all classes or varnas can be good or bad and gives a sociological explanation for how they arose, against the Brahmanical idea that they are divinely ordained.[365] According to Kancha Ilaiah, the Buddha posed the first contract theory of society.[366] The Buddha's teaching then is a single universal moral law, one Dharma valid for everybody, which is opposed to the Brahmanic ethic founded on "one's own duty" (svadharma) which depends on caste.[362] Because of this, all castes including untouchables were welcome in the Buddhist order and when someone joined, they renounced all caste affiliation.[367][368]

Socio-political teachings
The early texts depict the Buddha as giving a deflationary account of the importance of politics to human life. Politics is inevitable and is probably even necessary and helpful, but it is also a tremendous waste of time and effort, as well as being a prime temptation to allow ego to run rampant. Buddhist political theory denies that people have a moral duty to engage in politics except to a very minimal degree (pay the taxes, obey the laws, maybe vote in the elections), and it actively portrays engagement in politics and the pursuit of enlightenment as being conflicting paths in life.[369]

In the Aggañña Sutta, the Buddha teaches a history of how monarchy arose which according to Matthew J. Moore is "closely analogous to a social contract." The Aggañña Sutta also provides a social explanation of how different classes arose, in contrast to the Vedic views on social caste.[370]

Other early texts like the Cakkavatti-Sīhanāda Sutta and the Mahāsudassana Sutta focus on the figure of the righteous wheel turning leader (Cakkavatti). This ideal leader is one who promotes Dharma through his governance. He can only achieve his status through moral purity and must promote morality and Dharma to maintain his position. According to the Cakkavatti-Sīhanāda Sutta, the key duties of a Cakkavatti are: "establish guard, ward, and protection according to Dhamma for your own household, your troops, your nobles, and vassals, for Brahmins and householders, town and country folk, ascetics and Brahmins, for beasts and birds. let no crime prevail in your kingdom, and to those who are in need, give property."[370] The sutta explains the injunction to give to the needy by telling how a line of wheel-turning monarchs falls because they fail to give to the needy, and thus the kingdom falls into infighting as poverty increases, which then leads to stealing and violence.[an]

In the Mahāparinibbāna Sutta, the Buddha outlines several principles that he promoted among the Vajjika tribal federation, which had a quasi-republican form of government. He taught them to "hold regular and frequent assemblies", live in harmony and maintain their traditions. The Buddha then goes on to promote a similar kind of republican style of government among the Buddhist Sangha, where all monks had equal rights to attend open meetings and there would be no single leader, since The Buddha also chose not to appoint one.[370] Some scholars have argued that this fact signals that the Buddha preferred a republican form of government, while others disagree with this position.[370]

Worldly happiness
As noted by Bhikkhu Bodhi, the Buddha as depicted in the Pali suttas does not exclusively teach a world transcending goal, but also teaches laypersons how to achieve worldly happiness (sukha).[371]

According to Bodhi, the "most comprehensive" of the suttas that focus on how to live as a layperson is the Sigālovāda Sutta (DN 31). This sutta outlines how a layperson behaves towards six basic social relationships: "parents and children, teacher and pupils, husband and wife, friend and friend, employer and workers, lay follower and religious guides."[372] This Pali text also has parallels in Chinese and in Sanskrit fragments.[373][374]

In another sutta (Dīghajāṇu Sutta, AN 8.54) the Buddha teaches two types of happiness. First, there is the happiness visible in this very life. The Buddha states that four things lead to this happiness: "The accomplishment of persistent effort, the accomplishment of protection, good friendship, and balanced living."[375] Similarly, in several other suttas, the Buddha teaches on how to improve family relationships, particularly on the importance of filial love and gratitude as well as marital well-being.[376]

Regarding the happiness of the next life, the Buddha (in the Dīghajāṇu Sutta) states that the virtues which lead to a good rebirth are: faith (in the Buddha and the teachings), moral discipline, especially keeping the five precepts, generosity, and wisdom (knowledge of the arising and passing of things).[377]

According to the Buddha of the suttas then, achieving a good rebirth is based on cultivating wholesome or skillful (kusala) karma, which leads to a good result, and avoiding unwholesome (akusala) karma. A common list of good karmas taught by the Buddha is the list of ten courses of action (kammapatha) as outlined in MN 41 Saleyyaka Sutta (and its Chinese parallel in SĀ 1042).[378][379]

Good karma is also termed merit (puñña), and the Buddha outlines three bases of meritorious actions: giving, moral discipline and meditation (as seen in AN 8:36).[380]

Physical characteristics
Main article: Physical characteristics of the Buddha

Buddhist monks from Nepal. According to the earliest sources, the Buddha looked like a typical shaved man from northeast India.
Early sources depict the Buddha's as similar to other Buddhist monks. Various discourses describe how he "cut off his hair and beard" when renouncing the world. Likewise, Digha Nikaya 3 has a Brahmin describe the Buddha as a shaved or bald (mundaka) man.[381] Digha Nikaya 2 also describes how king Ajatashatru is unable to tell which of the monks is the Buddha when approaching the sangha and must ask his minister to point him out. Likewise, in MN 140, a mendicant who sees himself as a follower of the Buddha meets the Buddha in person but is unable to recognize him.[382]

The Buddha is also described as being handsome and with a clear complexion (Digha I:115; Anguttara I:181), at least in his youth. In old age, however, he is described as having a stooped body, with slack and wrinkled limbs.[383]

Various Buddhist texts attribute to the Buddha a series of extraordinary physical characteristics, known as "the 32 Signs of the Great Man" (Skt. mahāpuruṣa lakṣaṇa).

According to Anālayo, when they first appear in the Buddhist texts, these physical marks were initially held to be imperceptible to the ordinary person, and required special training to detect. Later though, they are depicted as being visible by regular people and as inspiring faith in the Buddha.[384]

These characteristics are described in the Digha Nikaya's Lakkhaṇa Sutta (D, I:142).[385]

In other religions
Main article: Gautama Buddha in world religions
Hinduism

Buddha incarnation of Vishnu, from Sunari, Medieval period. Gujari Mahal Archaeological Museum
Main article: Gautama Buddha in Hinduism
This Hindu synthesis emerged after the lifetime of the Buddha, between 500[386]–200[387] BCE and c. 300 CE,[386] under the pressure of the success of Buddhism and Jainism.[388] In response to the success of Buddhism, Gautama also came to be regarded as the 9th avatar of Vishnu.[121][389][390] Many Hindus claim that Buddha was Hindu and cite a belief that the Buddha is the ninth avatar of Vishnu in support.[ao] The adoption of the Buddha as an incarnation began at approximately the same time as Hinduism began to predominate and Buddhism to decline in India, the co-option into a list of avatars seen to be an aspect of Hindu efforts to decisively weaken Buddhist power and appeal in India.[392][393]

However, Buddha's teachings deny the authority of the Vedas and the concepts of Brahman-Atman.[394][395][396] Consequently, Buddhism is generally classified as a nāstika school (heterodox, literally "It is not so"[ap]) in contrast to the six orthodox schools of Hinduism.[399][400][401]

Islam
Islamic prophet Dhu al-Kifl has been identified with the Buddha based on Surah 95:1 of the Qur'an, which references a fig tree – a symbol that does not feature prominently in the lives of any of the other prophets mentioned in the Qur'an. It has meanwhile been suggested that the name Al-Kifl could be a reference to Kapilavastu, the home of Siddartha Gautama as a boy. [402]

Classical Sunni scholar Tabari reports that Buddhist idols were brought from Afghanistan to Baghdad in the ninth century. Such idols had been sold in Buddhist temples next to a mosque in Bukhara, but he does not further discuss the role of Buddha. According to the works on Buddhism by Al-Biruni (973–after 1050), views regarding the exact identity of Buddha were diverse. Accordingly, some regarded him as the divine incarnate, others as an apostle of the angels or as an Ifrit and others as an apostle of God sent to the human race. By the 12th century, al-Shahrastani even compared Buddha to Khidr, described as an ideal human. Ibn Nadim, who was also familiar with Manichaean teachings, even identifies Buddha as a prophet, who taught a religion to "banish Satan", although he does not mention it explicitly.[403]

The Buddha is also regarded as a prophet by the minority Ahmadiyya sect.[404]

Christianity
Main articles: Buddhism and Christianity, Buddhist influences on Christianity, and Comparison of Buddhism and Christianity

Christ and Buddha by Paul Ranson, 1880
The Christian Saint Josaphat is based on the Buddha. The name comes from the Sanskrit Bodhisattva via Arabic Būdhasaf and Georgian Iodasaph.[405] The only story in which St. Josaphat appears, Barlaam and Josaphat, is based on the life of the Buddha.[406] Josaphat was included in earlier editions of the Roman Martyrology (feast-day 27 November)—though not in the Roman Missal—and in the Eastern Orthodox Church liturgical calendar (26 August).

Other religions
In the Baháʼí Faith, Buddha is regarded as one of the Manifestations of God.

Some early Chinese Taoist-Buddhists thought the Buddha to be a reincarnation of Laozi.[407]

In the ancient Gnostic sect of Manichaeism, the Buddha is listed among the prophets who preached the word of God before Mani.[408]

In Sikhism, Buddha is mentioned as the 23rd avatar of Vishnu in the Chaubis Avtar, a composition in Dasam Granth traditionally and historically attributed to Guru Gobind Singh.[409]

Artistic depictions
Main article: Buddha in art
The earliest artistic depictions of the Buddha found at Bharhut and Sanchi are aniconic and symbolic. During this early aniconic period, the Buddha is depicted by other objects or symbols, such as an empty throne, a riderless horse, footprints, a Dharma wheel or a Bodhi tree.[410] Since aniconism precludes single devotional figures, most representations are of narrative scenes from his life. These continued to be very important after the Buddha's person could be shown, alongside larger statues. The art at Sanchi also depicts Jataka tales, narratives of the Buddha in his past lives.[411]

Other styles of Indian Buddhist art depict the Buddha in human form, either standing, sitting crossed legged (often in the Lotus Pose) or lying down on one side. Iconic representations of the Buddha became particularly popular and widespread after the first century CE.[412] Some of these depictions, particularly those of Gandharan Buddhism and Central Asian Buddhism, were influenced by Hellenistic art, a style known as Greco-Buddhist art.[413] The subsequently influenced the art of East Asian Buddhist images, as well as those of Southeast Asian Theravada Buddhism.

Gallery showing different Buddha styles
A Royal Couple Visits the Buddha, from railing of the Bharhut Stupa, Shunga dynasty, early 2nd century BC.
A Royal Couple Visits the Buddha, from railing of the Bharhut Stupa, Shunga dynasty, early 2nd century BC.

 
Adoration of the Diamond Throne and the Bodhi Tree, Bharhut.
Adoration of the Diamond Throne and the Bodhi Tree, Bharhut.

 
Descent of the Buddha from the Trayastrimsa Heaven, Sanchi Stupa No. 1.
Descent of the Buddha from the Trayastrimsa Heaven, Sanchi Stupa No. 1.

 
The Buddha's Miracle at Kapilavastu, Sanchi Stupa 1.
The Buddha's Miracle at Kapilavastu, Sanchi Stupa 1.

 
Bimbisara visiting the Buddha (represented as empty throne) at the Bamboo garden in Rajagriha
Bimbisara visiting the Buddha (represented as empty throne) at the Bamboo garden in Rajagriha

 
The great departure with riderless horse, Amaravati, 2nd century CE.
The great departure with riderless horse, Amaravati, 2nd century CE.

 
The Assault of Mara, Amaravati, 2nd century CE.
The Assault of Mara, Amaravati, 2nd century CE.

 
Isapur Buddha, one of the earliest physical depictions of the Buddha, c. 15 CE.[414] Art of Mathura
Isapur Buddha, one of the earliest physical depictions of the Buddha, c. 15 CE.[414] Art of Mathura

 
The Buddha attended by Indra at Indrasala Cave, Mathura 50-100 CE.
The Buddha attended by Indra at Indrasala Cave, Mathura 50-100 CE.

 
Buddha Preaching in Tushita Heaven. Amaravati, Satavahana period, 2d century CE. Indian Museum, Calcutta.
Buddha Preaching in Tushita Heaven. Amaravati, Satavahana period, 2d century CE. Indian Museum, Calcutta.

 
Standing Buddha from Gandhara.
Standing Buddha from Gandhara.

 
Gandharan Buddha with Vajrapani-Herakles.
Gandharan Buddha with Vajrapani-Herakles.

 
Kushan period Buddha Triad.
Kushan period Buddha Triad.

 
Buddha statue from Sanchi.
Buddha statue from Sanchi.

 
Birth of the Buddha, Kushan dynasty, late 2nd to early 3rd century CE.
Birth of the Buddha, Kushan dynasty, late 2nd to early 3rd century CE.

 
The Infant Buddha Taking A Bath, Gandhara 2nd century CE.
The Infant Buddha Taking A Bath, Gandhara 2nd century CE.

 
6th century Gandharan Buddha.
6th century Gandharan Buddha.

 
Buddha at Cave No. 6, Ajanta Caves.
Buddha at Cave No. 6, Ajanta Caves.

 
Standing Buddha, c. 5th Century CE.
Standing Buddha, c. 5th Century CE.

 
Sarnath standing Buddha, 5th century CE.
Sarnath standing Buddha, 5th century CE.

 
Seated Buddha, Gupta period.
Seated Buddha, Gupta period.

 
Seated Buddha at Gal Vihara, Sri Lanka.
Seated Buddha at Gal Vihara, Sri Lanka.

 
Chinese Stele with Sakyamuni and Bodhisattvas, Wei period, 536 CE.
Chinese Stele with Sakyamuni and Bodhisattvas, Wei period, 536 CE.

 
The Shakyamuni Daibutsu Bronze, c. 609, Nara, Japan.
The Shakyamuni Daibutsu Bronze, c. 609, Nara, Japan.

 
Amaravati style Buddha of Srivijaya period, Palembang, Indonesia, 7th century.
Amaravati style Buddha of Srivijaya period, Palembang, Indonesia, 7th century.

 
Korean Seokguram Cave Buddha, c. 774 CE.
Korean Seokguram Cave Buddha, c. 774 CE.

 
Seated Buddha Vairocana flanked by Avalokiteshvara and Vajrapani of Mendut temple, Central Java, Indonesia, early 9th century.
Seated Buddha Vairocana flanked by Avalokiteshvara and Vajrapani of Mendut temple, Central Java, Indonesia, early 9th century.

 
Buddha in the exposed stupa of Borobudur mandala, Central Java, Indonesia, c. 825.
Buddha in the exposed stupa of Borobudur mandala, Central Java, Indonesia, c. 825.

 
Vairocana Buddha of Srivijaya style, Southern Thailand, 9th century.
Vairocana Buddha of Srivijaya style, Southern Thailand, 9th century.

 
Seated Buddha, Japan, Heian period, 9th－10th century.
Seated Buddha, Japan, Heian period, 9th－10th century.

 
Attack of Mara, 10th century, Dunhuang.
Attack of Mara, 10th century, Dunhuang.

 
Cambodian Buddha with Mucalinda Nāga, c. 1100 CE, Banteay Chhmar, Cambodia
Cambodian Buddha with Mucalinda Nāga, c. 1100 CE, Banteay Chhmar, Cambodia

 
15th century Sukhothai Buddha.
15th century Sukhothai Buddha.

 
15th century Sukhothai Walking Buddha.
15th century Sukhothai Walking Buddha.

 
Sakyamuni, Lao Tzu, and Confucius, c. from 1368 until 1644.
Sakyamuni, Lao Tzu, and Confucius, c. from 1368 until 1644.

 
Chinese depiction of Shakyamuni, 1600.
Chinese depiction of Shakyamuni, 1600.

 
Shakyamuni Buddha with Avadana Legend Scenes, Tibetan, 19th century
Shakyamuni Buddha with Avadana Legend Scenes, Tibetan, 19th century

 
Golden Thai Buddha statue, Bodh Gaya.
Golden Thai Buddha statue, Bodh Gaya.

 
Gautama statue, Shanyuan Temple, Liaoning Province, China.
Gautama statue, Shanyuan Temple, Liaoning Province, China.

 
Burmese style Buddha, Shwedagon pagoda, Yangon.
Burmese style Buddha, Shwedagon pagoda, Yangon.

 
Large Gautama Buddha statue in Buddha Park of Ravangla.
Large Gautama Buddha statue in Buddha Park of Ravangla.

In other media
Films
Main article: Depictions of Gautama Buddha in film
Buddha Dev (Life of Lord Buddha), a 1923 Indian silent film by Dhundiraj Govind Phalke, first depiction of the Buddha on film with Bhaurao Datar in the titular role.[415]
Prem Sanyas (The Light of Asia), a 1925 silent film, directed by Franz Osten and Himansu Rai based on Arnold's epic poem with Rai also portraying the Buddha.[415]
Dedication of the Great Buddha (大仏開眼 or Daibutsu Kaigen), a 1952 Japanese feature film representing the life of Buddha.
Gotoma the Buddha, a 1957 Indian documentary film directed by Rajbans Khanna and produced by Bimal Roy.[415]
Siddhartha, a 1972 drama film by Conrad Rooks, an adaptation Hesse's novel. It stars Shashi Kapoor as Siddhartha, a contemporary of the Buddha.
Little Buddha, a 1994 film by Bernardo Bertolucci, the film stars Keanu Reeves as Prince Siddhartha.[415]
The Legend of Buddha, a 2004 Indian animated film by Shamboo Falke.
The Life of Buddha, or Prawat Phra Phuttajao, a 2007 Thai animated feature film about the life of Gautama Buddha, based on the Tipitaka.
Tathagatha Buddha, a 2008 Indian film by Allani Sridhar. Based on Sadguru Sivananda Murthy's book Gautama Buddha, it stars Sunil Sharma as the Buddha.[415]
Sri Siddhartha Gautama, a 2013 Sinhalese epic biographical film based on the life of Lord Buddha.
A Journey of Samyak Buddha, a 2013 Indian film by Praveen Damle, based on B. R. Ambedkar's 1957 Navayana book The Buddha and His Dhamma with Abhishek Urade in the titular role.
Television
Buddha, a 1996 Indian series which aired on Sony TV. It stars Arun Govil as the Buddha.[415]
Buddha, a 2013 Indian drama series on Zee TV starring Himanshu Soni in the titular role.
The Buddha 2010 PBS documentary by award-winning filmmaker David Grubin and narrated by Richard Gere.
Literature
The Light of Asia, an 1879 epic poem by Edwin Arnold
The Life of the Buddha: as it appears in the Pali Canon, the oldest authentic record, by Ñāṇamoli Bhikkhu (369 pp.) First printing 1972, fifth printing 2007
The Buddha and His Dhamma, a treatise on Buddha's life and philosophy, by B. R. Ambedkar
Before He Was Buddha: The Life of Siddhartha, by Hammalawa Saddhatissa
The Buddha and His Message: Past, Present & Future (United Nations Vesak Day Lecture), by Bhikkhu Bodhi (2000)
Buddha, a manga series that ran from 1972 to 1983 by Osamu Tezuka
Siddhartha novel by Hermann Hesse, written in German in 1922
Lord of Light, a novel by Roger Zelazny depicts a man in a far future Earth Colony who takes on the name and teachings of the Buddha
Creation, a 1981 novel by Gore Vidal, includes the Buddha as one of the religious figures that the main character encounters
Music
Karuna Nadee, a 2010 oratorio by Dinesh Subasinghe
The Light of Asia, an 1886 oratorio by Dudley Buck based on Arnold's poem
See also
Early Buddhist Texts
Dhammacakkappavattana Sutta
Anattalakkhaṇa Sutta
Samannaphala Sutta
Mahaparinibbana Sutta
Glossary of Buddhism
Great Renunciation & Four sights
Physical characteristics of the Buddha
Miracles of Buddha
Relics associated with Buddha
Lumbini, Bodhgaya, Sarnath & Kushinagar
Iconography of Gautama Buddha in Laos and Thailand
Knowing Buddha
Depictions of Gautama Buddha in film
Aniconism in Buddhism
List of Indian philosophers
References
Notes
 Buddha is seated cross-legged in the lotus position. In the centre of the base relief is a wheel symbolizing the dharmachakra, the Wheel of Buddhist law, with couchant deer on either side symbolizing the deer park in which the sermon was preached. The fingers of his hands form the teaching pose.
Sahni (1914, pp. 70–71, chapter B (b) 181): "Image (ht 5' 3 up to the top of the halo; width at base 2' 7) of Gautama Buddha seated cross-legged, preaching the first sermon at Sarnath, on a thick cushion supported on a seat with moulded legs."
Eck (1982, p. 63): In the most famous of these images in the Sarnath museum, the Buddha sits cross-legged, his limbs in the perfect proportions prescribed by the iconometry of the day, his hands in a teaching pose, his eyes downcast, half-shut in meditation, his head backed by a beautifully ornamented circular nimbus."
Mani (2012, pp. 66–67): "The seated Buddha, B(b) 181 showing Buddha cross-legged in the attitude of preaching, is one of the most exquisite creations of Gupta art. The halo is carved with a pair of celestial figures and conventionalized floral scroll-work."
 According to the Buddhist tradition, following the Nidanakatha (Fausböll, Davids & Davids 1878, p. [page needed]), the introductory to the Jataka tales, the stories of the former lives of the Buddha, Gautama was born in Lumbini, now in modern Nepal, but then part of the territory of the Shakya-clan.[120][122] In the mid-3rd century BCE the Emperor Ashoka determined that Lumbini was Gautama's birthplace and thus installed a pillar there with the inscription: "...this is where the Buddha, sage of the Śākyas (Śākyamuni), was born."(Gethin 1998, p. 19)

Based on stone inscriptions, there is also speculation that Lumbei, Kapileswar village, Odisha, at the east coast of India, was the site of ancient Lumbini.(Mahāpātra 1977; Mohāpātra 2000, p. 114; Tripathy 2014 Hartmann discusses the hypothesis and states, "The inscription has generally been considered spurious (...)"Hartmann 1991, pp. 38–39 He quotes Sircar: "There can hardly be any doubt that the people responsible for the Kapilesvara inscription copied it from the said facsimile not much earlier than 1928."

Kapilavastu was the place where he grew up:Keown & Prebish 2013, p. 436[aq]
Warder (2000, p. 45): "The Buddha [...] was born in the Sakya Republic, which was the city state of Kapilavastu, a very small state just inside the modern state boundary of Nepal against the Northern Indian frontier.
Walshe (1995, p. 20): "He belonged to the Sakya clan dwelling on the edge of the Himalayas, his actual birthplace being a few kilometres north of the present-day Northern Indian border, in Nepal. His father was, in fact, an elected chief of the clan rather than the king he was later made out to be, though his title was raja—a term which only partly corresponds to our word 'king'. Some of the states of North India at that time were kingdoms and others republics, and the Sakyan republic was subject to the powerful king of neighbouring Kosala, which lay to the south".
The exact location of ancient Kapilavastu is unknown.(Keown & Prebish 2013, p. 436) It may have been either Piprahwa in Uttar Pradesh, northern India (Nakamura 1980, p. 18; Srivastava 1979, pp. 61–74; Srivastava 1980, p. 108), or Tilaurakot (Tuladhar 2002, pp. 1–7), present-day Nepal (Huntington 1986, Keown & Prebish 2013, p. 436). The two cities are located only 24 kilometres (15 miles) from each other (Huntington 1986).
See also Conception and birth and Birthplace Sources
411–400: Dundas (2002), p. 24: "...as is now almost universally accepted by informed Indological scholarship, a re-examination of early Buddhist historical material, [...], necessitates a redating of the Buddha's death to between 411 and 400 BCE..."
405: Richard Gombrich[67][65][68]
Around 400: See the consensus in the essays by leading scholars in Narain (2003).
According to Pali scholar K. R. Norman, a life span for the Buddha of c. 480 to 400 BCE (and his teaching period roughly from c. 445 to 400 BCE) "fits the archaeological evidence better".[69] See also Notes on the Dates of the Buddha Íåkyamuni.
Indologist Michael Witzel provides a "revised" dating of 460–380 BCE for the lifetime of the Buddha.[70]
 According to Mahaparinibbana Sutta (see Äccess to insight," Maha-parinibbana Sutta), Gautama died in Kushinagar, which is located in present-day Uttar Pradesh, India.
 A number of names are being used to refer to the Buddha;
Siddhartha Gautama:
/sɪˈdɑːrtə, -θə/; Sanskrit: [sɪdːʱaːrtʰɐ ɡɐʊtɐmɐ] Gautama namely Gotama in Pali. Buswell & Lopez (2014, p. 817) "Siddhārtha": "Siddhārtha. (P. Siddhattha; T. Don grub; C. Xidaduo; J. Shiddatta/Shittatta; K. Siltalta ). In Sanskrit, "He Who Achieves His Goal," the personal name of GAUTAMA Buddha, also known as ŚĀKYAMUNI. In some accounts of the life of the Buddha, after his royal birth as the son of King ŚUDDHODANA, the BODHISATTVA was given this name and is referred to by that name during his life as a prince and his practice of asceticism. ... After his achievement of buddhahood, Siddhārtha is instead known as Gautama, Śākyamuni, or simply the TATHĀGATA."
Buswell & Lopez (2014, p. 316), "Gautama": "Gautama. (P.) Gotama; The family name of the historical Buddha, also known as ŚĀKYAMUNI Buddha. ... In Pāli literature, he is more commonly referred to as Gotama Buddha; in Mahāyāna texts, Śākyamuni Buddha is more common."
[Buddha] Shakyamuni:
Buswell & Lopez (2014, p. 741) "Śākyamuni": "Śākyamuni. (P. Sakkamuni; ... one of the most common epithets of GAUTAMA Buddha, especially in the MAHĀYĀNA traditions, where the name ŚĀKYAMUNI is used to distinguish the historical buddha from the myriad other buddhas who appear in the SŪTRAs."
Buddha Shakyamuni: from the middle of the 3rd century BCE, several Edicts of Ashoka (reigned c. 269–232 BCE) mention the Buddha and Buddhism (Bary (2011, p. 8), Fogelin (2015)). Particularly, Ashoka's Lumbini pillar inscription commemorates the Emperor's pilgrimage to Lumbini as the Buddha's birthplace, calling him the Buddha Shakyamuni (Brahmi script: 𑀩𑀼𑀥 𑀲𑀓𑁆𑀬𑀫𑀼𑀦𑀻 Bu-dha Sa-kya-mu-nī, "Buddha, Sage of the Shakyas") (In Ashoka's Rummindei Edict c. 260 BCE, in Hultzsch (1925, p. 164))
The Buddha:
Keown (2003, p. 42) chapter"Buddha (Skt; Pali)": "This is not a personal name but an epithet of those who have achieved enlightenment (*bodhi), the goal of the Buddhist religious life. Buddha comes from the *Sanskrit root 'budh', meaning to awaken, and the Buddhas are those who have awakened to the true nature of things as taught in the *Four Noble Truths. ... It is generally believed that there can never be more than one Buddha in any particular era, and the 'historical Buddha' of the present era was *Siddhartha Gautama. Numerous ahistorical Buddhas make an appearance in Mahayana literature."
"2013". Oxford English Dictionary (Online ed.). Oxford University Press. p. chapter "Buddha, n.". (Subscription or participating institution membership required.): "Also with the: (a title for) Siddhārtha Gautama, or Śākyamuni, a spiritual teacher from South Asia on whose teachings Buddhism is based, and who is believed to have been born in what is now Nepal and flourished in what is now Bihar, north-eastern India, during the 5th cent. b.c. Also: (a title given to) any Buddhist teacher regarded as having attained full awakening or enlightenment."
 The translation of "bodhi" and "Buddha" has shifted over time. While translated as "enlightenment" and "the enlightened one" since the 19th century, following Max Muller (Cohen 2006, p. 9), the preferred translation has shifted to "awakened" and "awakened one" (Bodhi 2020; Abrahams 2021:
Gimello (2003, p. entry "Bodhi (awakening"): "The Sanskrit and Pāli word bodhi derives from the Indic root [.radical] budh (to awaken, to know) [...] Those who are attentive to the more literal meaning of the Indic original tend to translate bodhi into English as "awakening," and this is to be recommended. However, it has long been conventional to translate it as "enlightenment," despite the risks of multiple misrepresentation attendant upon the use of so heavily freighted an English word."
Norman (1997, p. 29): "From the fourth jhana he gained bodhi. It is not at all clear what gaining bodhi means. We are accustomed to the translation "enlightenment" for bodhi, but this is misleading for two reasons. First, it can be confused with the use of the word to describe the development in European thought and culture in the eighteenth century, and second, it suggests that light is being shed on something, whereas there is no hint of the meaning "light" in the root budh- which underlies the word bodhi. The root means "to wake up, to be awake, to be awakened", and a buddha is someone who has been awakened. Besides the ordinary sense of being awakened by something, e.g. a noise, it can also mean "awakened to something". The desire to get the idea of "awakened" in English translations of buddha explains the rather peculiar Victorian quasi-poetical translation "the wake" which we sometimes find."
Bikkhu Bodhi objects to this shift: "The classical Pali text on grammar, Saddanīti, assigns to this root the meanings of “knowing (or understanding),” “blossoming,” and “waking up,” in that order of importance. The Pali-Sanskrit noun buddhi, which designates the intellect or faculty of cognition, is derived from budh, yet entails no sense of “awakening.” Further, when we look at the ordinary use of verbs based on budh in the Pali suttas, we can see that these verbs mean “to know, to understand, to recognize.” My paper cites several passages where rendering the verb as “awakens” would stretch the English word beyond its ordinary limits. In those contexts, “knows,” “understands,” “recognizes,” or “realizes” would fit much better. The verbs derived from budh that do mean “awaken” are generally preceded by a prefix, but they are not used to refer to the Buddha’s attainment of bodhi." (Bodhi 2020; Abrahams 2021)
Buddhadasa (2017, p. 5) gives several translations, including "the knowing one": "This is how we understand "Buddha" in Thailand, as the Awakened One, the Knowing One, and the Blossomed One."
 Buswell & Lopez 2014, p. entry "Sakyamuni" refer to the Ariyapariyesana Sutta, noting: "Buddha’s quest for enlightenment occurs in the ARIYAPARIYESANĀSUTTA. It is noteworthy that many of the most familiar events in the Buddha’s life are absent in some of the early accounts."
The Ariyapariyesana Sutta says: "So, at a later time, while still young, a black-haired young man endowed with the blessings of youth in the first stage of life — and while my parents, unwilling, were crying with tears streaming down their faces — I shaved off my hair & beard, put on the ochre robe and went forth from the home life into homelessness.
 See the Upaddha Sutta ("Half (of the Holy Life)"): "Admirable friendship, admirable companionship, admirable camaraderie is actually the whole of the holy life. When a monk has admirable people as friends, companions, & comrades, he can be expected to develop & pursue the noble eightfold path."[416]
 In Ashoka's Rummindei Edict c. 260 BCE, in Hultzsch (1925, p. 164)
 Minor Rock Edict Nb3: "These Dhamma texts – Extracts from the Discipline, the Noble Way of Life, the Fears to Come, the Poem on the Silent Sage, the Discourse on the Pure Life, Upatisa's Questions, and the Advice to Rahula which was spoken by the Buddha concerning false speech – these Dhamma texts, reverend sirs, I desire that all the monks and nuns may constantly listen to and remember. Likewise the laymen and laywomen."[42]

Dhammika: "There is disagreement amongst scholars concerning which Pali suttas correspond to some of the text. Vinaya samukose: probably the Atthavasa Vagga, Anguttara Nikaya, 1:98–100. Aliya vasani: either the Ariyavasa Sutta, Anguttara Nikaya, V:29, or the Ariyavamsa Sutta, Anguttara Nikaya, II: 27–28. Anagata bhayani: probably the Anagata Sutta, Anguttara Nikaya, III:100. Muni gatha: Muni Sutta, Sutta Nipata 207–21. Upatisa pasine: Sariputta Sutta, Sutta Nipata 955–75. Laghulavade: Rahulavada Sutta, Majjhima Nikaya, I:421."[42]
 In 2013, archaeologist Robert Coningham found the remains of a Bodhigara, a tree shrine, dated to 550 BCE at the Maya Devi Temple, Lumbini, speculating that it may possibly be a Buddhist shrine. If so, this may push back the Buddha's birth date.[73] Archaeologists caution that the shrine may represent pre-Buddhist tree worship, and that further research is needed.[73]
Richard Gombrich has dismissed Coningham's speculations as "a fantasy", noting that Coningham lacks the necessary expertise on the history of early Buddhism.[74]
Geoffrey Samuel notes that several locations of both early Buddhism and Jainism are closely related to Yaksha-worship, that several Yakshas were "converted" to Buddhism, a well-known example being Vajrapani,[75] and that several Yaksha-shrines, where trees were worshipped, were converted into Buddhist holy places.[76]
 Keay 2011: "The date [of Buddha's meeting with Bimbisara] (given the Buddhist 'short chronology') must have been around 400 BCE[...] He was now in the middle of his reign."
 Shakya:
Warder 2000, p. 45: "The Buddha [...] was born in the Sakya Republic, which was the city state of Kapilavastu, a very small state just inside the modern state boundary of Nepal against the Northern Indian frontier.
Walshe 1995, p. 20: "He belonged to the Sakya clan dwelling on the edge of the Himalayas, his actual birthplace being a few kilometres north of the present-day Northern Indian border, in Nepal. His father was, in fact, an elected chief of the clan rather than the king he was later made out to be, though his title was raja—a term which only partly corresponds to our word 'king'. Some of the states of North India at that time were kingdoms and others republics, and the Sakyan republic was subject to the powerful king of neighbouring Kosala, which lay to the south".
 According to Alexander Berzin, "Buddhism developed as a shramana school that accepted rebirth under the force of karma, while rejecting the existence of the type of soul that other schools asserted. In addition, the Buddha accepted as parts of the path to liberation the use of logic and reasoning, as well as ethical behaviour, but not to the degree of Jain asceticism. In this way, Buddhism avoided the extremes of the previous four shramana schools."[88]
 Based on stone inscriptions, there is also speculation that Lumbei, Kapileswar village, Odisha, at the east coast of India, was the site of ancient Lumbini.(Mahāpātra 1977Mohāpātra 2000, p. 114Tripathy 2014) Hartmann 1991, pp. 38–39 discusses the hypothesis and states, "The inscription has generally been considered spurious (...)" He quotes Sircar: "There can hardly be any doubt that the people responsible for the Kapilesvara inscription copied it from the said facsimile not much earlier than 1928."
 Some sources mention Kapilavastu as the birthplace of the Buddha. Gethin states: "The earliest Buddhist sources state that the future Buddha was born Siddhārtha Gautama (Pali Siddhattha Gotama), the son of a local chieftain—a rājan—in Kapilavastu (Pali Kapilavatthu) what is now the Indian–Nepalese border."[124] Gethin does not give references for this statement.
 According to Geoffrey Samuel, the Buddha was born into a Kshatriya clan,[133] in a moderate Vedic culture at the central Ganges Plain area, where the shramana-traditions developed. This area had a moderate Vedic culture, where the Kshatriyas were the highest varna, in contrast to the Brahmanic ideology of Kuru–Panchala, where the Brahmins had become the highest varna.[133] Both the Vedic culture and the shramana tradition contributed to the emergence of the so-called "Hindu-synthesis" around the start of the Common Era.[134][133]
 An account of these practices can be seen in the Mahāsaccaka-sutta (MN 36) and its various parallels (which according to Anālayo include some Sanskrit fragments, an individual Chinese translation, a sutra of the Ekottarika-āgama as well as sections of the Lalitavistara and the Mahāvastu).[180]
 According to various early texts like the Mahāsaccaka-sutta, and the Samaññaphala Sutta, a Buddha has achieved three higher knowledges: Remembering one's former abodes (i.e. past lives), the "Divine eye" (dibba-cakkhu), which allows the knowing of others' karmic destinations and the "extinction of mental intoxicants" (āsavakkhaya).[181][184]
 Scholars have noted inconsistencies in the presentations of the Buddha's enlightenment, and the Buddhist path to liberation, in the oldest sutras. These inconsistencies show that the Buddhist teachings evolved, either during the lifetime of the Buddha, or thereafter. See:
* Bareau (1963)
* Schmithausen (1981)
* Norman (2003)
* Vetter (1988)
* Gombrich (2006a), Chapter 4
* Bronkhorst (1993), Chapter 7
* Anderson (1999)
 Anālayo draws from seven early sources:[221]
the Dharmaguptaka Vinaya in Four Parts, preserved in Chinese
a *Vinayamātṛkā preserved in Chinese translation, which some scholars suggest represents the Haimavata tradition
the Mahāsāṃghika-Lokottaravāda Vinaya, preserved in Sanskrit
the Mahīśāsaka Vinaya in Five Parts, preserved in Chinese
the Mūlasarvāstivāda Vinaya, where the episode is extant in Chinese and Tibetan translation, with considerable parts also preserved in Sanskrit fragments
a discourse in the Madhyama-āgama, preserved in Chinese, probably representing the Sarvāstivāda tradition
a Pāli discourse found among the Eights of the Aṅguttara-nikāya; the same account is also found in the Theravāda Vinaya preserved in Pāli
 Waley notes: suukara-kanda, "pig-bulb"; suukara-paadika, "pig's foot" and sukaresh.ta "sought-out by pigs". He cites Neumann's suggestion that if a plant called "sought-out by pigs" exists then suukaramaddava can mean "pig's delight".
 One common basic list of twelve elements in the Early Buddhist Texts goes as follows: "Conditioned by (1) ignorance are (2) formations, conditioned by formations is (3) consciousness, conditioned by consciousness is (4) mind-and-body, conditioned by mind-and-body are (5) the six senses, conditioned by the six senses is (6) sense-contact, conditioned by sense-contact is (7) feeling, conditioned by feeling is (8) craving, conditioned by craving is (9) grasping, conditioned by grasping is (10) becoming, conditioned by becoming is (11) birth, conditioned by birth is (12) old-age and death-grief, lamentation, pain, sorrow, and despair come into being. Thus is the arising of this whole mass of suffering."[284]
 Shulman refers to Schmitthausen (2000), Zur Zwolfgliedrigen Formel des Entstehens in Abhangigkeit, in Horin: Vergleichende Studien zur Japanischen Kultur, 7
 Gombrich: "The six senses, and thence, via 'contact' and 'feeling', to thirst." It is quite plausible, however, that someone failed to notice that once the first four links became part of the chain, its negative version meant that in order to abolish ignorance one first had to abolish consciousness!"[295]
 right view; right intention, right speech, right action, right livelihood, right effort, right mindfulness, and right concentration.[315]
 Early texts that outline the graduated path include the Cula-Hatthipadopama-sutta (MN 27, with Chinese parallel at MĀ 146) and the Tevijja Sutta (DN 13, with Chinese parallel at DĀ 26 and a fragmentary Sanskrit parallel entitled the Vāsiṣṭha-sūtra).[318][319][320]
Gethin adds: "This schema is assumed and, in one way or another, adapted by the later manuals such as the Visuddhimagga, the Abhidharmakosa, Kamalasila's Bhavanakrama ('Stages of Meditation', eighth century) and also Chinese and later Tibetan works such as Chih-i's Mo-ho chih-kuan ('Great Calm and Insight') and Hsiu-hsi chih-kuan tso-ch'an fa-yao ('The Essentials for Sitting in Meditation and Cultivating Calm and Insight', sixth century), sGam-po-pa's Thar-pa rin-po che'i rgyan ('Jewel Ornament of Liberation', twelfth century) and Tsong-kha-pa's Lam rim chen mo ('Great Graduated Path', fourteenth century).[321]
 As Gethin notes: "A significant ancient variation on the formula of dependent arising, having detailed the standard sequence of conditions leading to the arising of this whole mass of suffering, thus goes on to state that: Conditioned by (1) suffering, there is (2) faith, conditioned by faith, there is (3) gladness, conditioned by gladness, there is (4) joy, conditioned by joy, there is (5) tranquillity, conditioned by tranquillity, there is (6) happiness, conditioned by happiness, there is (7) concentration, conditioned by concentration, there is (8) knowledge and vision of what truly is, conditioned by knowledge and vision of what truly is, there is (9) disenchantment, conditioned by disenchantment, there is (10) dispassion, conditioned by dispassion, there is (11) freedom, conditioned by freedom, there is (12) knowledge that the defilements are destroyed."[323]
 For a comparative survey of Satipatthana in the Pali, Tibetan and Chinese sources, see: Anālayo (2014). Perspectives on Satipatthana.[full citation needed]. For a comparative survey of Anapanasati, see: Dhammajoti, K.L. (2008). "Sixteen-mode Mindfulness of Breathing". JCBSSL. VI.[full citation needed].
 Understanding of these marks helps in the development of detachment:
Anicca (Sanskrit: anitya): That all things that come to have an end;
Dukkha (Sanskrit: duḥkha): That nothing which comes to be is ultimately satisfying;
Anattā (Sanskrit: anātman): That nothing in the realm of experience can really be said to be "I" or "mine".
 Two well-known proponent of this position are A.K. Warder and Richard Gombrich.
According to A.K. Warder, in his 1970 publication Indian Buddhism, "from the oldest extant texts a common kernel can be drawn out."[341] According to Warder, c.q. his publisher: "This kernel of doctrine is presumably common Buddhism of the period before the great schisms of the fourth and third centuries BCE. It may be substantially the Buddhism of the Buddha himself, although this cannot be proved: at any rate it is a Buddhism presupposed by the schools as existing about a hundred years after the parinirvana of the Buddha, and there is no evidence to suggest that it was formulated by anyone else than the Buddha and his immediate followers".[341]
Richard Gombrich: "I have the greatest difficulty in accepting that the main edifice is not the work of a single genius. By "the main edifice" I mean the collections of the main body of sermons, the four Nikāyas, and of the main body of monastic rules."[339]
 A proponent of the second position is Ronald Davidson.
Ronald Davidson: "While most scholars agree that there was a rough body of sacred literature (disputed) [sic] that a relatively early community (disputed) [sic] maintained and transmitted, we have little confidence that much, if any, of surviving Buddhist scripture is actually the word of the historical Buddha."[342]
 Well-known proponents of the third position are:
J.W. de Jong: "It would be hypocritical to assert that nothing can be said about the doctrine of earliest Buddhism [...] the basic ideas of Buddhism found in the canonical writings could very well have been proclaimed by him [the Buddha], transmitted and developed by his disciples and, finally, codified in fixed formulas."[343]
Johannes Bronkhorst: "This position is to be preferred to (ii) for purely methodological reasons: only those who seek may find, even if no success is guaranteed."[340]
Donald Lopez: "The original teachings of the historical Buddha are extremely difficult, if not impossible, to recover or reconstruct."[344]
 Exemplary studies are the study on descriptions of "liberating insight" by Lambert Schmithausen,[349] the overview of early Buddhism by Tilmann Vetter,[337] the philological work on the four truths by K.R. Norman,[350] the textual studies by Richard Gombrich,[339] and the research on early meditation methods by Johannes Bronkhorst.[332]
 Vetter: "However, if we look at the last, and in my opinion the most important, component of this list [the noble eightfold path], we are still dealing with what according to me is the real content of the middle way, dhyana-meditation, at least the stages two to four, which are said to be free of contemplation and reflection. Everything preceding the eighth part, i.e. right samadhi, apparently has the function of preparing for the right samadhi."[352]
 aggihuttamukhā yaññā sāvittī chandaso mukham. Sacrifices have the Agnihotra as foremost; of meter, the foremost is the Sāvitrī.[361]
 "Not by water man becomes pure; people here bathe too much; in whom there is truth and morality, he is pure, he is (really) a brahman"[362]
 "These three things, monks, are conducted in secret, not openly. What three? Affairs with women, the mantras of the brahmins, and wrong view. But these three things, monks, shine openly, not in secret. What three? The moon, the sun, and the Dhamma and Discipline proclaimed by the Tathagata." AN 3.129[363]
 "In a favourite stanza quoted several times in the Pali Canon: "The Kshatriya is the best among those people who believe in lineage; but he, who is endowed with knowledge and good conduct, is the best among Gods and men".[362]
 "thus, from the not giving of property to the needy, poverty became rife, from the growth of poverty, the taking of what was not given increased, from the increase of theft, the use of weapons increased, from the increased use of weapons, the taking of life increased — and from the increase in the taking of life, people's life-span decreased, their beauty decreased, and [as] a result of this decrease of life-span and beauty, the children of those whose life-span had been eighty-thousand years lived for only forty thousand."[370]
 This belief is not universally held as Krishna is held to be the ninth avatar in some traditions and his half-brother Balarama the eight.[391]
 "in Sanskrit philosophical literature, 'āstika' means 'one who believes in the authority of the Vedas', 'soul', 'Brahman'. ('nāstika' means the opposite of these).[397][398]
 Some sources mention Kapilavastu as the birthplace of the Buddha. Gethin states: "The earliest Buddhist sources state that the future Buddha was born Siddhārtha Gautama (Pali Siddhattha Gotama), the son of a local chieftain—a rājan—in Kapilavastu (Pali Kapilavatthu) what is now Nepal."Gethin 1998, p. 14 Gethin does not give references for this statement.
Cite error: A list-defined reference has no name (see the help page).
Citations
 Cousins (1996), pp. 57–63.
 Norman (1997), p. 33.
 Prebish (2008).
 Ray 1999, p. 65-67.
 Buswell & Lopez 2014, p. entry "Sakyamuni".
 Laumakis (2008), p. 4.
 Gethin (1998), pp. 40–41.
 Warder (2000), pp. 4–7, 44.
 Warder (2000), p. 4.
 Cox (2003), p. 1–7.
 Donald Lopez Jr., The Scientific Buddha: His Short and Happy Life, Yale University Press, p.24
 Buswell & Lopez 2014, p. 817.
 Bopearachchi, Osmund (1 January 2021). "GREEK HELIOS OR INDIAN SŪRYA? THE SPREAD OF THE SUN GOD IMAGERY FROM INDIA TO GANDHĀRA". Connecting the Ancient West and East. Studies Presented to Prof. Gocha R. Tsetskhladze, Edited by J. Boardman, J. Hargrave, A. Avram and A. Podossinov, Monographs in Antiquity: 946.
 Witzel, Michael (2012). "Ṛṣis". Brill's Encyclopedia of Hinduism Online. Brill.
 Macdonell, Arthur Anthony; Keith, Arthur Berriedale (1912). Vedic Index of Names and Subjects. Vol. 1. John Murray. p. 240.
 Bary (2011), p. 8.
 Fogelin (2015).
 Hultzsch (1925), p. 164.
 Gethin (1998), p. 8.
 Buswell & Lopez 2014, p. 398.
 Sir Monier Monier-Williams; Ernst Leumann; Carl Cappeller (2002). A Sanskrit-English Dictionary: Etymologically and Philologically Arranged with Special Reference to Cognate Indo-European Languages. Motilal Banarsidass. p. 733. ISBN 978-81-208-3105-6.
 Keown (2003), p. 42.
 Buswell & Lopez 2014, p. 398, entry "Buddha".
 Baroni (2002), p. 230.
 Buswell & Lopez 2014, p. Entry "Tathāgata".
 Chalmers, Robert. The Journal of the Royal Asiatic Society, 1898. pp.103-115 Archived 13 August 2012 at the Wayback Machine
 Peter Harvey, The Selfless Mind. Curzon Press 1995, p.227
 Dhammananda, Ven. Dr. K. Sri, Great Virtues of the Buddha (PDF), Dhamma talks
 Roshen Dalal (2014). The Religions of India: A Concise Guide to Nine Major Faiths. Penguin Books. ISBN 9788184753967. Entry: "Jina"
 Snyder, David N. (2006) "The Complete Book of Buddha's Lists--explained." Vipassana Foundation, list 605 p. 429.
 von Hinüber (2008), pp. 198–206.
 Witzel, Michael (2009). "Moving Targets? Texts, language, archaeology and history in the Late Vedic and early Buddhist periods". Indo-Iranian Journal. 52 (2–3): 287–310. doi:10.1163/001972409X12562030836859. S2CID 154283219.
 Strong (2001), p. 5.
 Weise (2013), pp. 46–47.
 Bronkhorst, Johannes (2016). "How the Brahmins Won: Appendix X Was there Buddhism in Gandhāra at the Time of Alexander?". How the Brahmins Won. Brill: 483–489, page 6 of the appendix. doi:10.1163/9789004315518_016.
 Beckwith, Christopher I. (2017). Greek Buddha: Pyrrho's Encounter with Early Buddhism in Central Asia. Princeton University Press. p. 168. ISBN 978-0-691-17632-1.
 Leoshko, Janice (2017). Sacred Traces: British Explorations of Buddhism in South Asia. Routledge. p. 64. ISBN 978-1-351-55030-7.
 Sarao, K. T. S. (16 September 2020). The History of Mahabodhi Temple at Bodh Gaya. Springer Nature. p. 80. ISBN 978-981-15-8067-3.
 Irwin, after Cunningham, has Bhagavato Saka Munino Bodhi, "The Bodhi (Tree) of the Buddha Sakya Muni" in Irwin, John (1 January 1990). "The 'Tree-of-Life' in Indian Sculpture". South Asian Studies. 6 (1): 33. doi:10.1080/02666030.1990.9628398. ISSN 0266-6030.
 Prebish, Charles S. (1 November 2010). Buddhism: A Modern Perspective. Penn State Press. p. 29. ISBN 978-0-271-03803-2.
 "Definition of dhamma". Dictionary.com. Retrieved 27 October 2020.
 Dhammika (1993).
 "That the True Dhamma Might Last a Long Time: Readings Selected by King Asoka". Access to Insight. Translated by Bhikkhu, Thanissaro. 1993. Retrieved 8 January 2016.
 "Ancient Buddhist Scrolls from Gandhara". UW Press. Archived from the original on 27 May 2017. Retrieved 4 September 2008.
 Schober (2002), p. 20.
 Fowler (2005), p. 32.
 Beal (1883).
 Cowell (1894).
 Willemen (2009).
 Olivelle, Patrick (2008). Life of the Buddha by Ashva-ghosha (1st ed.). New York: New York University Press. p. xix. ISBN 978-0-8147-6216-5.
 Karetzky (2000), p. xxi.
 Beal (1875).
 Swearer (2004), p. 177.
 Smith (1924), pp. 34, 48.
 Schumann (2003), pp. 1–5.
 Buswell (2003), p. 352.
 Lopez (1995), p. 16.
 Wynne, Alexander. "Was the Buddha an awakened prince or a humble itinerant?". Aeon. Retrieved 9 May 2020.
 Strong, John, ix-x in "Forward" to The Thousand and One Lives of the Buddha, by Bernard Faure, 2022, University of Hawaii Press, ISBN 9780824893545, google books Archived 2 November 2022 at the Wayback Machine
 Das, Sarat Chandra (1882). Contributions on the Religion and History of Tibet. First published in: Journal of the Asiatic Society of Bengal, Vol. LI. Reprint: Manjushri Publishing House, Delhi. 1970, pp. 81–82 footnote 6.
 Reynolds & Hallisey (2005), p. 1061.
 Schumann (2003), pp. 10–13.
 Bechert 1991–1997,[full citation needed].
 Ruegg (1999), pp. 82–87.
 Narain (1993), pp. 187–201.
 Prebish (2008), p. 2.
 Gombrich (1992).
 Gombrich (2000).
 Norman (1997), p. 39.
 Witzel, Michael (2019). "Early 'Aryans' and their neighbors outside and inside India". Journal of Biosciences. 44 (3): 58. doi:10.1007/s12038-019-9881-7. ISSN 0973-7138. PMID 31389347. S2CID 195804491.
 Schumann (2003), p. xv.
 Wayman (1997), pp. 37–58.
 Vergano, Dan (25 November 2013). "Oldest Buddhist Shrine Uncovered In Nepal May Push Back the Buddha's Birth Date". National Geographic. Retrieved 26 November 2013.
 Gombrich (2013).
 Tan, Piya (21 December 2009), Ambaṭṭha Sutta. Theme: Religious arrogance versus spiritual openness (PDF), Dharma farer, archived from the original (PDF) on 9 January 2016, retrieved 22 October 2014
 Samuel (2010), pp. 140–152.
 Rawlinson (1950), p. 46.
 Muller (2001), p. xlvii.
 Sharma 2006.
 Keay (2011).
 Gombrich (1988), p. 49.
 Levman, Bryan Geoffrey (2013). "Cultural Remnants of the Indigenous Peoples in the Buddhist Scriptures". Buddhist Studies Review. 30 (2): 145–180. ISSN 1747-9681. Archived from the original on 1 November 2020. Retrieved 23 February 2020.
 Bronkhorst, J. (2007). "Greater Magadha, Studies in the culture of Early India," p. 6. Leiden, Boston, MA: Brill. doi:10.1163/ej.9789004157194.i-416
 Jayatilleke (1963), chpt. 1–3.
 Clasquin-Johnson, Michel. "Will the real Nigantha Nātaputta please stand up? Reflections on the Buddha and his contemporaries". Journal for the Study of Religion. 28 (1): 100–114. ISSN 1011-7601.
 Walshe (1995), p. 268.
 Collins (2009), pp. 199–200.
 Berzin, Alexander (April 2007). "Indian Society and Thought before and at the Time of Buddha". Study Buddhism. Retrieved 20 June 2016.
 Nakamura (1980), p. 20.
 Wynne (2007), pp. 8–23, ch. 2.
 Warder (1998), p. 45.
 Roy (1984), p. 1.
 Roy (1984), p. 7.
 Coningham & Young 2015, p. 65.
 Thapar 2004, p. 169.
 Dyson 2019.
 Ludden 1985.
 Stein & Arnold 2012, p. 62.
 Bronkhorst 2011, p. 1.
 Fogelin 2015, p. 74.
 Yaldiz, Marianne (1987). Investigating Indian Art. Staatl. Museen Preuss. Kulturbesitz. p. 188. The earliest anthropomorphic representation of the Buddha that we know so far, the Bimaran reliquary
 Verma, Archana (2007). Cultural and Visual Flux at Early Historical Bagh in Central India. Archana Verma. p. 1. ISBN 978-1-4073-0151-8.
 Anālayo (2006).
 Tan, Piya (trans) (2010). "The Discourse to Sandaka (trans. of Sandaka Sutta, Majjhima Nikāya 2, Majjhima Paṇṇāsaka 3, Paribbājaka Vagga 6)" (PDF). The Dharmafarers. The Minding Centre. pp. 17–18. Archived from the original (PDF) on 9 January 2016. Retrieved 24 September 2015.
 MN 71 Tevijjavacchagotta [Tevijjavaccha]
 "A Sketch of the Buddha's Life: Readings from the Pali Canon". Access to Insight. 2005. Retrieved 24 September 2015.
 Jones (1956), p. [page needed].
 Skilton (2004), pp. 64–65.
 Carrithers (2001), p. 15.
 Armstrong (2000), p. xii.
 Carrithers (2001), p. [page needed].
 Strong (2001), p. 19.
 Strong (2001), p. 21.
 Strong (2001), p. 24.
 Strong (2001), p. 30.
 Strong (2001), p. 31.
 Strong (2001), p. 25.
 Strong (2001), p. 37.
 Strong (2001), p. 43.
 "Lumbini, the Birthplace of the Lord Buddha". World Heritage Convention. UNESCO. Retrieved 26 May 2011.
 Nagendra, Kumar Singh (1997). "Buddha as depicted in the Purāṇas". Encyclopaedia of Hinduism. Vol. 7. Anmol Publications. pp. 260–275. ISBN 978-81-7488-168-7. Retrieved 16 April 2012.
 "The Astamahapratiharya: Buddhist pilgrimage sites". Victoria and Albert Museum. Archived from the original on 31 October 2012. Retrieved 25 December 2012.
 Keown & Prebish (2013), p. 436.
 Gethin (1998), p. 14.
 Trainor (2010), pp. 436–437.
 Nakamura (1980), p. 18.
 Huntington (1986).
 Gethin (1998), p. 19.
 Beal (1875), p. 37.
 Jones (1952), p. 11.
 Beal (1875), p. 41.
 Hirakawa (1990), p. 21.
 Samuel (2010).
 Hiltebeitel (2013).
 Warder (2000), p. 45.
 Ñāṇamoli Bhikkhu (1992), p. 8.
 Strong (2001), p. 51.
 Hirakawa (1990), p. 24.
 Dhammika (n.d.), p. [page needed].
 Gethin (1998), pp. 14–15.
 Gombrich (1988), pp. 49–50.
 Thapar (2002), p. 146.
 Turpie (2001), p. 3.
 Narada (1992), pp. 9–12.
 Strong (2001), p. 55.
 Narada (1992), pp. 11–12.
 Hamilton (2000), p. 47.
 Meeks (2016), p. 139.
 Schumann (2003), p. 23.
 Strong (2001), p. 60.
 Gethin (1998), p. 15.
 Anālayo (2011), p. 170.
 Wynne, Alexander (2019). "Did the Buddha exist?". JOCBS. 16: 98–148.
 Schumann (2003), p. 45.
 Schumann (2003), pp. 45–46.
 Anālayo (2011), p. 173.
 Gethin (1998), p. 21.
 Strong (2001), p. 63.
 Gethin (1998), p. 20.
 Conze (1959), pp. 39–40.
 Warder (2000), p. 322.
 Schumann (2003), p. 44.
 Strong (2001), Incitements to Leave Home.
 Strong (2015), The Beginnings of Discontent.
 Narada (1992), pp. 15–16.
 Strong (2015), The Great Departure.
 Penner (2009), p. 28.
 Strong (2001), The Great Departure.
 Hirakawa (1990), p. 25.
 Marshall (1918), p. 65.
 Ñāṇamoli Bhikkhu (1992), p. 15.
 Upadhyaya (1971), p. 95.
 Laumakis (2008), p. 8.
 Schumann (2003), p. 47.
 Anālayo (2011), p. 175.
 Schumann (2003), p. 48.
 Armstrong (2000), p. 77.
 Narada (1992), pp. 19–20.
 Hirakawa (1990), p. 26.
 Anālayo (2011), pp. 234–235.
 Anālayo (2011), p. 236.
 Anālayo (2011), p. 240.
 "The Golden Bowl". Life of the Buddha. Retrieved 25 December 2012 – via BuddhaNet.
 "Maha-Saccaka Sutta: The Longer Discourse to Saccaka". Access to Insight. Translated by Bhikkhu, Thanissaro. 2008. (MN 36). Retrieved 19 May 2007.
 Anālayo (2011), p. 243.
 Anderson (1999).
 Williams (2002), pp. 74–75.
 Lopez, Donald. "Four Noble Truths". Encyclopædia Britannica.
 "Dhammacakkappavattana Sutta: Setting the Wheel of Dhamma in Motion". Access to Insight. Translated by Bhikkhu, Thanissaro. 1993. Retrieved 25 December 2012.
 "nirvana". Encyclopædia Britannica. Retrieved 22 October 2014.
 Anālayo (2011), p. 178.
 Gyatso (2007), pp. 8–9.
 Ñāṇamoli Bhikkhu (1992), p. 30.
 Ñāṇamoli Bhikkhu (1992), pp. 30–35.
 Strong (2001), p. 93.
 Strong (2001), p. 94.
 Anālayo (2011), p. 182.
 Anālayo (2011), p. 183.
 Boisselier, Jean (1994). The wisdom of the Buddha. New York: Harry N. Abrams. ISBN 0-8109-2807-8. OCLC 31489012.
 Anālayo (2011), p. 185.
 Ñāṇamoli Bhikkhu (1992), pp. 44–45.
 Strong (2001), p. 110.
 Strong (2001), p. 113.
 Ñāṇamoli Bhikkhu (1992), pp. 48, 54–59.
 Strong (2001), pp. 116–117.
 Ñāṇamoli Bhikkhu (1992), p. 64.
 Strong (2001), p. 115.
 Malalasekera (1960), pp. 291–292.
 Strong (2001), p. 131.
 Schumann (2003), p. 231.
 Strong (2001), p. 132.
 Bhikkhu Khantipalo (1995). "Lay Buddhist Practice, The Shrine Room, Uposatha Day, Rains Residence Archived 2 November 2022 at the Wayback Machine"
 Ñāṇamoli Bhikkhu (1992), p. 68.
 Ñāṇamoli Bhikkhu (1992), p. 70.
 Strong (2001), p. 119.
 Ñāṇamoli Bhikkhu (1992), p. 78.
 Ñāṇamoli Bhikkhu (1992), pp. 79–83.
 Strong (2001), p. 122.
 Ñāṇamoli Bhikkhu (1992), p. 91.
 Strong (2001), p. 136.
 Anālayo (2016), pp. 40–41.
 Anālayo (2016), p. 43.
 Anālayo (2016), p. 79.
 Anālayo (2013b).
 Anālayo (2016), pp. 111–112.
 Anālayo (2016), p. 127.
 Strong (2001), p. 134.
 Schumann (2003), pp. 232–233.
 Jain (1991), p. 79.
 Mahajan, V.D. (2016). Ancient India. S. Chand Publishing. p. 190.
 Schumann (2003), p. 215.
 Schumann (2003), p. 232.
 Anālayo (2011), p. 198.
 Ñāṇamoli Bhikkhu (1992), p. 257.
 Schumann (2003), p. 236.
 Schumann (2003), p. 237.
 Bhikkhu Sujato (2012), "Why Devadatta Was No Saint, A critique of Reginald Ray's thesis of the 'condemned saint'" Archived 30 January 2020 at the Wayback Machine
 Ñāṇamoli Bhikkhu (1992), p. 280.
 Schumann (2003), p. 239.
 Strong (2001), p. 165.
 Anālayo (2014).
 Ñāṇamoli Bhikkhu (1992), pp. 286–288.
 Strong (2001), pp. 165–166.
 Schumann (2003), p. 244.
 Schumann (2003), p. 246.
 "Maha-parinibbana Sutta", Digha Nikaya, Access insight, verse 56
 Bhikkhu & von Hinüber (2000).
 Bhikkhu, Mettanando (15 May 2001). "How the Buddha died". Bangkok Post. Archived from the original on 14 November 2012. Retrieved 25 December 2012 – via BuddhaNet.
 Waley (1932), pp. 343–354.
 Strong (2001), p. 176.
 Schumann (2003), p. 249.
 Strong (2001), p. 178.
 Schumann (2003), p. 250.
 Wynne (2007), p. 112.
 Strong (2001), p. 183.
 Ñāṇamoli Bhikkhu (1992), p. 324.
 Ñāṇamoli Bhikkhu (1992), p. 327.
 Ñāṇamoli Bhikkhu (1992), p. 330.
 Ñāṇamoli Bhikkhu (1992), p. 331.
 Lopez, Donald. "The Buddha's relics". Encyclopædia Britannica.
 Strong (2007), pp. 136–137.
 Harvey, Peter (2013), An Introduction to Buddhism: Teachings, History and Practices (PDF) (2nd ed.), New York: Cambridge University Press, p. 88, ISBN 978-0-521-85942-4
 Reat, Noble Ross (1996). "The Historical Buddha and his Teachings". In Potter, Karl H. (ed.). Encyclopedia of Indian Philosophy, Vol. VII: Abhidharma Buddhism to 150 AD. Motilal Banarsidass. pp. 28, 33, 37, 41, 43, 48.
 Anālayo (2011), p. 891.
 Salomon, Richard (20 January 2020). "How the Gandharan Manuscripts Change Buddhist History". Lion's Roar. Retrieved 21 January 2020.
 Bodhi (2005), p. 39.
 Bodhi (2005), pp. 32–33.
 Gethin (1998), p. 59.
 Siderits (2019).
 Gethin (1998), p. 61.
 Gethin (1998), p. 62.
 Gombrich (2009), p. 12.
 Gombrich (2009), p. 19.
 Gombrich (2009), p. 20.
 Gombrich (2009), p. 49.
 Gombrich (2009), p. 13.
 Gethin (1998), p. 135.
 Gombrich (2009), p. 114.
 Steven M. Emmanuel (2015). A Companion to Buddhist Philosophy. John Wiley & Sons. pp. 587–588. ISBN 978-1-119-14466-3.
 Skandha Archived 3 January 2018 at the Wayback Machine Encyclopædia Britannica (2013)
 Karunamuni ND (May 2015). "The Five-Aggregate Model of the Mind". SAGE Open. 5 (2): 215824401558386. doi:10.1177/2158244015583860.
 Hamilton (2000), p. 22.
 Gombrich (2009), p. 131.
 Gethin (1998), pp. 141–142.
 Frauwallner 1973, pp. 167–168.
 Hajime Nakamura. The Theory of ‘Dependent Origination’ in its Incipient Stage in Somaratana Balasooriya, Andre Bareau, Richard Gombrich, Siri Gunasingha, Udaya Mallawarachchi, Edmund Perry (Editors) (1980) "Buddhist Studies in Honor of Walpola Rahula." London.
 Shulman 2008, p. 305, note 19.
 Wayman 1984a, p. 173 with note 16.
 Wayman 1984b, p. 256.
 Wayman 1971.
 David J. Kalupahana (1975). Causality: The Central Philosophy of Buddhism. University of Hawaii Press. pp. 6–7. ISBN 978-0-8248-0298-1.
 Gombrich 2009, pp. 135–136.
 Jurewicz 2000.
 Boisvert 1995, pp. 147–150.
 Gombrich 2009, p. 138.
 Gombrich (2009), pp. 9, 67.
 Gombrich (2009), p. 10.
 Hamilton (2000), pp. 19–20.
 Andrew Glass, Mark Allon (2007). "Four Gandhari Samyuktagama Sutras", pp. 5, 15.
 Mun-keat Choong (2000), "The Fundamental Teachings of Early Buddhism: A Comparative Study Based on the Sutranga Portion of the Pali Samyutta-Nikaya and the Chinese Samyuktagama", Otto Harrassowitz Verlag, p. 59.
 Gombrich (2009), pp. 119–120.
 Gethin (1998), pp. 136–137.
 Gethin (1998), pp. 146–147.
 Gethin (1998), p. 148.
 Hamilton (2000), p. 27.
 Gethin (1998), p. 139.
 Gethin (1998), pp. 134–135.
 Hamilton (2000), p. 20.
 Gombrich (2009), pp. 62–64.
 Gombrich (2009), pp. 73–74.
 Bodhi (2005), p. 229.
 Anālayo (2013a).
 Gethin (1998), pp. 63–64.
 Gethin (1998), p. 81.
 Gethin (1998), p. 164.
 Gethin (1998), pp. 217–218.
 Gethin (1998), pp. 83, 165.
 Bucknell (1984).
 Anālayo (2011), p. 189.
 Anālayo (2015).
 Gethin (1998), p. 165.
 Bodhi, Bhikkhu (1995). Transcendental Dependent Arising. A Translation and Exposition of the Upanisa Sutta Archived 6 December 2019 at the Wayback Machine.
 Gethin (1998), p. 157.
 Vetter (1988), p. 5.
 Anālayo (2017a), pp. 80, 128, 135.
 Wynne (2004), pp. 23, 37.
 Bronkhorst (1993), p. 10.
 Gombrich (2009), pp. 9, 36.
 Gombrich (2009), p. 48.
 Norman (1997), p. 26.
 Norman (1997), p. 28.
 Bronkhorst (1993).
 Vetter (1988), p. ix.
 Warder (2000), p. [page needed].
 Tse-Fu Kuan. "Mindfulness in similes in Early Buddhist literature". In Edo Shonin; William Van Gordon; Nirbhay N. Singh (eds.). Buddhist Foundations of Mindfulness. p. 267.
 Mun-Keat Choong (1999). The Notion of Emptiness in Early Buddhism. Motilal Banarsidass. p. 3.
 Vetter (1988).
 Schmithausen (1990).
 Gombrich (1997).
 Bronkhorst (1993), p. vii.
 Warder (2000), inside flap.
 Davidson (2003), p. 147.
 Jong (1993), p. 25.
 Lopez (1995), p. 4.
 Warder (2004), p. [page needed].
 Gombrich (2006b), p. 21.
 Harvey, Peter (1990). "An Introduction to Buddhism: Teachings, History and Practices," p. 3. Introduction to Religion. Cambridge University Press.
 Wynne, Alexander (2005). "The Historical Authenticity of Early Buddhist Literature". Vienna Journal of South Asian Studies. XLIX: 35–70.
 Schmithausen (1981).
 Norman (2003).
 Vetter (1988), pp. xxx, xxxv–xxxvi, 4–5.
 Vetter (1988), p. xxx.
 Vetter (1988), pp. xxxiv–xxxvii.
 Bronkhorst (1993), p. 107.
 Conze, Edward (2000). "Buddhism: A Short History." From Buddhism to Sufism Series. Oneworld.
 Gethin (1998), pp. 85, 88.
 Kalupahana (1992), p. 28.
 Gethin (1998), p. 85.
 Heirman, Ann (2019). "Vinaya rules for monks and nuns."
 Gethin (1998), p. 87.
 Shults (2014), p. 119.
 Tola, Fernando. Dragonetti, Carmen (2009). "Brahamanism and Buddhism: Two Antithetic Conceptions of Society in Ancient India." p. 26: "This also implied the denial of the Shruti provided with characteristics which grant it the status of a substance. All this carried with itself also the negation of the authority of all the sacred texts of Brahmanism. Buddhism does not acknowledge to them any value as ultimate criterion of truth, as depository of the norms which regulate man's conduct as a member of society and in his relations with the Gods. Buddhism ignores the Shruti, the very foundation of Brahmanism."
 Bodhi (2005), pp. 33–34.
 Omvedt (2003), p. 76.
 Omvedt (2003), p. 72.
 Omvedt, Gail (1 June 2001). "Review: The Buddha as a Political Philosopher". Economic and Political Weekly. Vol. 36, no. 21. pp. 1801–1804. JSTOR 4410659.
 Mrozik, Susanne. "Upali" in MacMillan Encyclopedia of Buddhism, pg. 870.
 Kancha Ilaiah, "God as Political Philosopher: Buddha's Challenge to Brahminism" p. 169
 Moore, Matthew J. (2016). Buddhism and Political Theory. Oxford University Press. p. 2. ISBN 978-0-19-046551-3.
 Moore, Matthew J. (2015). "Political theory in Canonical Buddhism". Philosophy East & West. 65 (1): 36–64. doi:10.1353/pew.2015.0002. S2CID 143618675.
 Bodhi (2005), pp. 107–109.
 Bodhi (2005), p. 109.
 Pannasiri, Bhadanta (1950). "Sigālovāda-Sutta", Visva-Bharati Annals, 3: 150–228.
 Martini, Giuliana (2013). "Bodhisattva Texts, Ideologies and Rituals in Khotan in the Fifth and Sixth Centuries", in Buddhism among the Iranian Peoples of Central Asia, M. De Chiara et al. (ed.), 11–67, Wien: Österreichische Akademie der Wissenschaften.
 Bodhi (2005), p. 124.
 Bodhi (2005), p. 110.
 Bodhi (2005), pp. 111, 125.
 Bodhi (2005), pp. 146–148, 156.
 Anālayo (2011), p. 263.
 Bodhi (2005), pp. 151, 167.
 Olivelle, Patrick (1974), "The Origin and the Early Development of Buddhist Monachism", p. 19.
 Mazard, Eisel (2010). "The Buddha was bald," Archived 3 February 2020 at the Wayback Machine New Mandala.
 Dhammika (n.d.), pp. 23–24.
 Anālayo (2017b), pp. 137–138.
 Walshe (1995), pp. 441–460.
 Hiltebeitel 2013, p. 12.
 Larson 1995.
 Vijay Nath 2001, p. 21.
 Gopal (1990), p. 73.
 Doniger (1993), p. 243.
 Britannica, Eds Encycl (19 February 2015), "Balaram", Encyclopedia Britannica, retrieved 17 April 2022, Balarama, in Hindu mythology, the elder half brother of Krishna, with whom he shared many adventures. Sometimes Balarama is considered one of the 10 avatars (incarnations) of the god Vishnu, particularly among those members of Vaishnava sects who elevate Krishna to the rank of a principal god.
 Muesse, Mark W. (2016), "Crossing Boundaries:When Founders of Faith Appear in Other Traditions", in Gray, Patrick (ed.), Varieties of Religious Invention: Founders and Their Functions in History, New York: Oxford University Press, p. 184, ISBN 978-0-19-935971-4, Although orthodox Hinduism regards Buddhism as a nastika darshana, a heterodox (sometimes translated as "atheistic") philosophy, many modern Hindus nevertheless wish to include Gotama as part of the Hindu traditions. Gandhi, for example, insisted that the Buddha was a Hindu, a claim that many Hindus today affirm. The traditional belief that the Buddha was the ninth avatar of the god Vishnu, one of the cosmic deities of Hinduism, is often cied in support of this view. Many Hindus who claim the Buddha as one of their own, however, fail to recognize the ambivalence of this tradition. ... The adoption of Buddha as an incarnation of Vishnu seems to have commenced at roughly the same time Hinduism gained in ascendancy in India and Buddhism began to decline. Thus, the Hindu inclusion of the Buddha in this traditional list of Vishnu's ten avatars may in fact represent a part of Hindu efforts to eviscerate Buddhist power and appeal.
 Doniger, Wendy (30 September 2010). The Hindus: An Alternative History. OUP Oxford. pp. 481–484. ISBN 978-0-19-959334-7.
 "Buddha". Stanford Encyclopedia of Philosophy. Retrieved 13 July 2015.
 Sushil Mittal & Gene Thursby (2004), The Hindu World, Routledge, ISBN 978-0-415-77227-3, pp. 729–730
 C. Sharma (2013), A Critical Survey of Indian Philosophy, Motilal Banarsidass, ISBN 978-81-208-0365-7, p. 66
 Andrew J. Nicholson (2013), Unifying Hinduism: Philosophy and Identity in Indian Intellectual History, Columbia University Press, ISBN 978-0-231-14987-7, Chapter 9
 Ghurye, G.S. (2011). S. Devadas Pillai (ed.). Indian Sociology Through Ghurye, a Dictionary. p. 354. ISBN 978-81-7154-807-1. OCLC 38215769.
 Ambedkar, B.R. "Book One, Part V – The Buddha and His Predecessors". The Buddha and his Dharma.
 Williams, Paul; Tribe, Anthony (2000). Buddhist thought a complete introduction to the Indian tradition. London: Taylor & Francis e-Library. pp. 1–10. ISBN 0-203-18593-5.
 Flood (1996), pp. 231–232.
 Yusuf (2009), pp. 376.
 Ahmad Faizuddin Ramli; Jaffary Awang; Zaizul Ab Rahman (2018). Muslim scholar's discourse on Buddhism: a literature on Buddha's position. International Conference on Humanities and Social Sciences (ICHSS 2018). SHS Web of Conferences. Vol. 53, no. 4001. pp. 6–7. doi:10.1051/shsconf/20185304001.
 Valentine, Simon Ross (2008), Islam and the Ahmadiyya Jamaʻat: History, Belief, Practice, p. 26, ISBN 978-0-231-70094-8, retrieved 15 November 2013
 Macdonnel (1900).
 Mershman (1913).
 Twitchett (1986).
 Barnstone W & Meyer M (2009). The Gnostic Bible: Gnostic texts of mystical wisdom from the ancient and medieval worlds. Shambhala Publications: Boston & London.
 "Chaubis Avtar". www.info-sikh.com. Archived from the original on 1 June 2003.
 Leidy (2008), p. 15.
 Leidy (2008), p. 19.
 Leidy (2008), p. 31.
 Marshall (1960), pp. 1–40.
 Quintanilla, Sonya Rhie (2007). History of Early Stone Sculpture at Mathura: Ca. 150 BCE – 100 CE. BRILL. pp. 199–206, 204 for the exact date. ISBN 9789004155374.
 Bakker, Freek L. (30 September 2009). The Challenge of the Silver Screen: An Analysis of the Cinematic Portraits of Jesus, Rama, Buddha and Muhammad. BRILL. p. 135. ISBN 9789004194045.
Sources
Abrahams, Matthew (2021), "In Defense of "Enlightenment". "Awakening" has become the preferred English term for the Buddha's attainment. But has something gotten lost in translation? Ven. Bhikkhu Bodhi in conversation with Matthew Abrahams", TriCycle
Anālayo, Bhikkhu (2006). "The Buddha and Omniscience". Indian International Journal of Buddhist Studies. 7: 1–20.
——— (2011). A Comparative Study of the Majjhima-nikāya Volume 1 (Introduction, Studies of Discourses 1 to 90.
——— (2013a). "The Chinese Parallels to the Dhammacakkappavattana-sutta (2)". Journal of the Oxford Centre for Buddhist Studies (5): 9–41.
——— (2013b). "The Gurudharmaon Bhikṣuṇī Ordination in the Mūlasarvāstivāda Tradition". Journal of Buddhist Ethics. 20: 752. ISSN 1076-9005.
——— (2014). "The Buddha's Last Meditation in the Dirgha-Agama". The Indian International Journal of Buddhist Studies. 15.
——— (2015). "Brahmavihāra and Awakening, A Study of the Dīrgha-āgama Parallel to the Tevijja-sutta". Asian Literature and Translation. 3 (4): 1–27. doi:10.18573/j.2015.10216.
——— (2016). The Foundation History of the Nun's Order. projekt verlag, Bochum/Freiburg. ISBN 978-3-89733-387-1.
——— (2017a). Early Buddhist Meditation Studies. Barre Center for Buddhist Studies. ISBN 978-1-5404-1050-4.
——— (2017b). Buddhapada and the Bodhisattva Path (PDF). Hamburg Buddhist Studies. Vol. 8. projekt verlag, Bochum/Freiburg. ISBN 978-3-89733-415-1.
Anderson, Carol (1999), Pain and Its Ending: The Four Noble Truths in the Theravada Buddhist Canon, Routledge
Armstrong, Karen (2000), Buddha, Orion, ISBN 978-0-7538-1340-9
Asvaghosa (1883), The Fo-sho-hing-tsan-king, a life of Buddha, translated by Beal, Samuel, Oxford: Clarendon
Bareau, André (1963), Recherches sur la biographie du Buddha dans les Sutrapitaka et les Vinayapitaka anciens (in French), Ecole Francaise d'Extreme-Orient
Baroni, Helen J. (2002), The Illustrated Encyclopedia of Zen Buddhism, Rosen
Bary, William Theodore de (16 March 2011). The Buddhist Tradition: In India, China and Japan. Knopf Doubleday Publishing Group. p. 8. ISBN 978-0-307-77879-6.
Beal, Samuel (1875), The romantic legend of Sâkya Buddha (Abhiniṣkramaṇa Sūtra), London: Trübner
Bechert, Heinz, ed. (1991–1997), The dating of the historical Buddha (Symposium), vol. 1–3, Göttingen: Vandenhoeck & Ruprecht
———, ed. (1991). The Dating of the Historical Buddha. Vol. 1. Göttingen: Vandenhoeck and Ruprecht.
———, ed. (1992). Die Datierung des historischen Buddha [The Dating of the Historical Buddha]. Symposien zur Buddhismusforschung, IV (in German). Vol. 2. Gottingen: Vandenhoeck and Ruprecht.
Bhikkhu, Mettanando; von Hinüber, Oskar (2000), "The Cause of the Buddha's Death" (PDF), Journal of the Pali Text Society, XXVI: 105–118, archived from the original (PDF) on 9 April 2015
Bodhi, Bhikkhu (2005), In the Buddha's Words: An Anthology of Discourses from the Pali Canon, Simon and Schuster
Bodhi, Bhikkhu (2020), "On Translating "Buddha"", Journal of the Oxford Centre for Buddhist Studies
Boisvert, Mathieu (1995), The Five Aggregates: Understanding Theravada Psychology and Soteriology, Wilfrid Laurier University Press, ISBN 978-0-88920-257-3
Bronkhorst, Johannes (1993), The Two Traditions of Meditation In Ancient India, Motilal Banarsidass
Bronkhorst, Johannes (2011), Buddhism in the Shadow of Brahmanism, BRILL
Bucknell, Robert S. (1984), "The Buddhist path to liberation: An analysis of the listing of stages", The Journal of the International Association of Buddhist Studies
Buddhadasa (2017), Under the Bodhi Tree: Buddha's Original Vision of Dependent Co-arising, Wisdom Publications
Buswell, Robert E., ed. (2003), Encyclopedia of Buddhism, vol. 1, US: Macmillan Reference, ISBN 978-0-02-865910-7
Buswell, Robert E. Jr.; Lopez, Donald S. Jr., eds. (2014), The Princeton Dictionary of Buddhism, Princeton and Oxford: Princeton University Press, ISBN 978-0-691-15786-3
Carrithers, M. (2001), The Buddha: A Very Short Introduction, Oxford University Press, ISBN 978-0-02-865910-7
Cohen, Robert S. (2006), Beyond Enlightenment: Buddhism, Religion, Modernity, Routledge
Collins, Randall (2009), The Sociology of Philosophies, Harvard University Press, ISBN 978-0-674-02977-4, 1120 pp.
Coningham, Robin; Young, Ruth (2015), The Archaeology of South Asia: From the Indus to Asoka, c. 6500 BCE–200 CE, Cambridge University Press, ISBN 978-0-521-84697-4
Conze, Edward, trans. (1959), Buddhist Scriptures, London: Penguin
Cousins, L.S. (1996). "The Dating of the Historical Buddha: A Review Article". Journal of the Royal Asiatic Society. 3. 6 (1): 57–63. doi:10.1017/s1356186300014760. ISSN 1356-1863. JSTOR 25183119. S2CID 162929573. Archived from the original on 26 February 2011. Retrieved 4 April 2006 – via Indology.
Cowell, Edward Byles, transl. (1894), "The Buddha-Karita of Ashvaghosa", in Müller, Max (ed.), Sacred Books of the East, vol. XLIX, Oxford: Clarendon
Cox, Collett (2003), "Abidharma", in Buswell, Robert E. (ed.), Encyclopedia of Buddhism, New York: Macmillan Reference Lib., ISBN 0028657187
Davidson, Ronald M. (2003), Indian Esoteric Buddhism, Columbia University Press, ISBN 978-0-231-12618-2
de Bary, William (1969). The Buddhist Tradition in India, China and Japan (February 1972 ed.). xvii: Vintage Books. p. xvii. ISBN 0-394-71696-5.
Dhammika, Shravasti (n.d.) [1990s]. The Buddha & his disciples. Singapore: Buddha Dhamma Mandala Society. ISBN 981-00-4525-5. OCLC 173196980.
——— (1993), The Edicts of King Asoka: An English Rendering, The Wheel Publication, Kandy, Sri Lanka: Buddhist Publication Society, ISBN 978-955-24-0104-6, archived from the original on 28 October 2013
Doniger, Wendy, ed. (1993), Purana Perennis: Reciprocity and Transformation in Hindu and Jaina Texts, State University of New York Press, ISBN 0-7914-1381-0
Dundas, Paul (2002), The Jains (2nd ed.), Routledge, ISBN 978-0-415-26606-2, retrieved 25 December 2012
Dyson, Tim (2019), A Population History of India: From the First Modern People to the Present Day, Oxford University Press
Eck, Diana L. (1982), Banāras, City of Light, New York: Alfred A. Knopf, p. 63, ISBN 0-394-51971-X
Fausböll, V. (1878), Buddhist birth-stories (Jataka tales), translated by T.W. Rhys Davids, (new & rev. ed. by C.A. Rhys Davids), London: George Routledge & Sons Ltd.; New York: E.P. Dutton & Co.
Flood, Gavin D. (1996). An Introduction to Hinduism. Cambridge University Press. ISBN 978-0-521-43878-0.
Fogelin, Lars (1 April 2015). An Archaeological History of Indian Buddhism. Oxford University Press. ISBN 978-0-19-026692-9.
Fowler, Mark (2005), Zen Buddhism: beliefs and practices, Sussex Academic Press
Frauwallner, Erich (1973), "Chapter 5. The Buddha and the Jina", History of Indian Philosophy: The philosophy of the Veda and of the epic. The Buddha and the Jina. The Sāmkhya and the classical Yoga-system, Motilal Banarsidass
Gethin, Rupert, M.L. (1998), Foundations of Buddhism, Oxford University Press
Gimello, Robert M. (2003), "Bodhi (awakening)", in Buswell, Robert E. (ed.), Encyclopedia of Buddhism, vol. 1, US: Macmillan Reference, ISBN 978-0-02-865910-7
Gombrich, Richard F. (1988), Theravada Buddhism: A Social History from Ancient Benares to Modern Colombo, Routledge and Kegan Paul
———. "Dating the Buddha: a red herring revealed". In Bechert (1992), pp. 237–259..
——— (1997), How Buddhism Began, Munshiram Manoharlal
——— (2000), "Discovering the Buddha's date", in Perera, Lakshman S (ed.), Buddhism for the New Millennium, London: World Buddhist Foundation, pp. 9–25.
——— (2006a). How Buddhism Began: The Conditioned Genesis of the Early Teachings. Routledge. ISBN 978-1-134-19639-5.
——— (2006b), Theravada Buddhism: A Social History from Ancient Benares to Modern Colombo, The Library of Religious Beliefs and Practices Series, Routledge and Kegan Paul, ISBN 978-1-134-21718-2
——— (2009), What the Buddha thought, Equinox
——— (12 December 2013). "Recent discovery of "earliest Buddhist shrine" a sham?". Tricycle.
Gopal, Madan (1990), K.S. Gautam (ed.), India through the ages, Publication Division, Ministry of Information and Broadcasting, Government of India, p. 73
Gyatso, Geshe Kelsang (2007), Introduction to Buddhism An Explanation of the Buddhist Way of Life, Tharpa, ISBN 978-0-9789067-7-1
Hamilton, Sue (2000), Early Buddhism: A New Approach: The I of the Beholder, Routledge
Hartmann, Jens Uwe. "Research on the date of the Buddha: South Asian Studies Published in Western Languages". In Bechert (1991), pp. 27–45.
Hiltebeitel, Alf (2013) [2002], "Hinduism", in Kitagawa, Joseph (ed.), The Religious Traditions of Asia: Religion, History, and Culture, Routledge, ISBN 978-1-136-87597-7
Hirakawa, Akira (1990), A History of Indian Buddhism: From Śākyamuni to Early Mahāyāna, University of Hawaii Press, hdl:10125/23030, ISBN 0-8248-1203-4
Hultzsch, E. (1925). Inscriptions of Asoka (in Sanskrit). p. 164.
Huntington, John C. (September 1986), "Sowing the Seeds of the Lotus: A Journey to the Great Pilgrimage Sites of Buddhism, part V" (PDF), Orientations, 17 (9): 46–58, archived (PDF) from the original on 28 November 2014
Jain, Kailash Chand (1991), Lord Mahāvīra and His Times, Motilal Banarsidass, ISBN 978-81-208-0805-8
Jayatilleke, K.N. (1963), Early Buddhist Theory of Knowledge (1st ed.), London: George Allen & Unwin Ltd.
Jones, J.J. (1952), The Mahāvastu, Sacred Books of the Buddhists, vol. 2, London: Luzac & Co.
——— (1956), The Mahāvastu, Sacred Books of the Buddhists, vol. 3, London: Luzac & Co.
Jong, JW de (1993), "The Beginnings of Buddhism", The Eastern Buddhist, 26 (2)
Jurewicz, Joanna (2000), "Playing with Fire: The pratityasamutpada from the perspective of Vedic thought" (PDF), Journal of the Pali Text Society, 26: 77–103
Kalupahana, David (1992), A History of Buddhist Philosophy: Continuities and Discontinuities, University of Hawaii Press
Karetzky, Patricia (2000), Early Buddhist Narrative Art, Lanham, MD: University Press of America
Keay, John (2011). India: A History. New York: Grove Press. ISBN 978-0-8021-9550-0.
Keown, Damien, ed. (2003), "Buddha (Skt; Pali)", A Dictionary of Buddhism, Oxford University Press, ISBN 0-19-860560-9
Keown, Damien; Prebish, Charles S (2013), Encyclopedia of Buddhism, Routledge
Larson, Gerald (1995), India's Agony Over Religion, SUNY Press, ISBN 978-0-7914-2411-7
Laumakis, Stephen (2008), An Introduction to Buddhist philosophy, Cambridge; New York: Cambridge University Press, ISBN 978-0-521-85413-9
Leidy, Denise Patty (2008). The Art of Buddhism: An Introduction to Its History & Meaning. Shambhala Publications.
Lopez, Donald S. (1995), Buddhism in Practice, Princeton University Press, ISBN 978-0-691-04442-2
Ludden, David (1985), India and South Asia
Macdonnel, Arthur Anthony (1900), "Wikisource-logo.svg Sanskrit Literature and the West.", A History of Sanskrit Literature, New York: D Appleton & Co
Mahāpātra, Cakradhara (1977), The real birth place of Buddha, Grantha Mandir
Malalasekera, G.P. (1960), Dictionary of Pali Proper Names, Vol. 1, London: Pali Text Society/Luzac, ISBN 9788120618237
Mani, B. R. (2012) [2006], Sarnath: Archaeology, Art, and Architecture, New Delhi: The Director General: Archaeological Survey of India, pp. 66–67
Marshall, John (1918). A Guide To Sanchi.
——— (1960). The Buddhist art of Gandhāra: the story of the early school, its birth, growth and decline. Memoirs of the Department of archaeology in Pakistan. Vol. 1. Cambridge.</ref>
Meeks, Lori (27 June 2016), "Imagining Rāhula in Medieval Japan" (PDF), Japanese Journal of Religious Studies, 43 (1): 131–151, doi:10.18874/jjrs.43.1.2016.131-151
Mohāpātra, Gopinath (2000), "Two Birth Plates of Buddha" (PDF), Indologica Taurinensia, 26: 113–119, archived from the original (PDF) on 4 October 2012
Mershman, Francis (1913), "Barlaam and Josaphat", in Herberman, Charles G; et al. (eds.), The Catholic Encyclopedia, vol. 2, New York: Robert Appleton
Muller, F. Max (2001), The Dhammapada and Sutta-nipata, Routledge (UK), ISBN 0-7007-1548-7
Nakamura, Hajime (1980), Indian Buddhism: a survey with bibliographical notes, Motilal Banarsidass, ISBN 978-81-208-0272-8
Narada (1992), A Manual of Buddhism, Buddha Educational Foundation, ISBN 978-967-9920-58-1
Narain, A.K. (1993), "Book Review: Heinz Bechert (ed.), The dating of the Historical Buddha, part I", Journal of the International Association of Buddhist Studies, 16 (1): 187–201
———, ed. (2003). The Date of the Historical Śākyamuni Buddha. New Delhi: BR Publishing. ISBN 8176463531.
Nath, Vijay (2001), "From 'Brahmanism' to 'Hinduism': Negotiating the Myth of the Great Tradition", Social Scientist, 29 (3/4): 19–50, doi:10.2307/3518337, JSTOR 3518337
Norman, K.R. (1997), A Philological Approach to Buddhism, The Bukkyo Dendo Kyokai Lectures 1994, School of Oriental and African Studies (University of London)
——— (2003), "The Four Noble Truths", K.R. Norman Collected Papers, vol. II, Oxford: Pali Text Society, pp. 210–223
Ñāṇamoli Bhikkhu (1992), The Life of the Buddha: According to the Pali Canon, Buddhist Publication Society
OED (2013), "Buddha, n.", Oxford English Dictionary (3 ed.), Oxford University Press
Omvedt, Gail (2003). Buddhism in India: Challenging Brahmanism and Caste. SAGE. ISBN 978-0-7619-9664-4.
Penner, Hans H. (2009), Rediscovering the Buddha: The Legends and Their Interpretations, Oxford University Press, ISBN 978-0-19-538582-3
Prebish, Charles S. (2008), "Cooking the Buddhist Books: The Implications of the New Dating of the Buddha for the History of Early Indian Buddhism" (PDF), Journal of Buddhist Ethics, 15: 1–21, ISSN 1076-9005, archived from the original (PDF) on 28 January 2012
Rawlinson, Hugh George (1950), A Concise History of the Indian People, Oxford University Press
Ray, Reginald A. (1999), Buddhist Saints in India: A Study in Buddhist Values and Orientations, Oxford University Press
Reynolds, Frank E.; Hallisey, Charles (2005), "Buddha", in Jones, Lindsay (ed.), MacMillan Encyclopedia of Religion Vol.2, MacMillan
Roy, Ashim Kumar (1984), A history of the Jains, New Delhi: Gitanjali, p. 179, CiteSeerX 10.1.1.132.6107
Ruegg, Seyford (1999), "A new publication on the date and historiography of Buddha's decease (nirvana): a review article", Bulletin of the School of Oriental and African Studies, University of London, 62 (1): 82–87, doi:10.1017/s0041977x00017572, S2CID 162902049
Sahni, Daya Ram (1914), "B (b) 181.", Catalogue of the Museum of Archaeology at Sarnath, Calcutta: Superintendent Government Printing, India, pp. 70–71, OCLC 173481241
Samuel, Geoffrey (2010), The Origins of Yoga and Tantra. Indic Religions to the Thirteenth Century, Cambridge University Press
Schmithausen, Lambert (1981), "On some Aspects of Descriptions or Theories of 'Liberating Insight' and 'Enlightenment' in Early Buddhism", in von Klaus, Bruhn; Wezler, Albrecht (eds.), Studien zum Jainismus und Buddhismus (Gedenkschrift für Ludwig Alsdorf) [Studies on Jainism and Buddhism (Schriftfest for Ludwig Alsdorf)] (in German), Wiesbaden, pp. 199–250
——— (1990), Buddhism and Nature, Tokyo, OCLC 697272229
Schober, Juliane (2002), Sacred biography in the Buddhist traditions of South and Southeast Asia, Delhi: Motilal Banarsidass
Schumann, Hans Wolfgang (2003), The Historical Buddha: The Times, Life, and Teachings of the Founder of Buddhism, Motilal Banarsidass, ISBN 978-81-208-1817-0
Sharma, R.S. (2006), India's Ancient Past, Oxford University Press
Shulman, Eviatar (2008), "Early Meanings of Dependent-Origination" (PDF), Journal of Indian Philosophy, 36 (2): 297–317, doi:10.1007/s10781-007-9030-8, S2CID 59132368
Shults, Brett (2014), "On the Buddha's Use of Some Brahmanical Motifs in Pali Texts", Journal of the Oxford Centre for Buddhist Studies, 6: 106–140
Siderits, Mark (2019), "Buddha", The Stanford Encyclopedia of Philosophy, Metaphysics Research Lab, Stanford University
Srivastava, K.M. (1979), "Kapilavastu and Its Precise Location", East and West, 29 (1/4): 61–74
——— (1980), "Archaeological Excavations at Priprahwa and Ganwaria and the Identification of Kapilavastu", Journal of the International Association of Buddhist Studies, 3 (1): 103–110
Skilton, Andrew (2004), A Concise History of Buddhism
Smith, Vincent (1924), The Early History of India (4th ed.), Oxford: Clarendon
Stein, Burton; Arnold, David (2012), A History of India, Oxford-Wiley
Strong, J.S. (2001), The Buddha: A Beginner's Guide, Oneworld Publications, ISBN 978-1-78074-054-6
——— (2007), Relics of the Buddha, Motilal Banarsidass
——— (2015), Buddhisms: An Introduction, Oneworld Publications, ISBN 978-1-78074-506-0
Swearer, Donald (2004), Becoming the Buddha, Princeton, NJ: Princeton University Press
Thapar, Romila (2002), The Penguin History of Early India: From Origins to AD 1300, Penguin
Thapar, Romila (2004), Early India: From the Origins to AD 1300, University of Californian Press, ISBN 0-520-24225-4
Trainor, Kevin (2010), "Kapilavastu", in: Keown, Damien; Prebish, Charles S. Encyclopedia of Buddhism, London: Routledge, ISBN 978-1-136-98588-1
Tripathy, Ajit Kumar (January 2014), "The Real Birth Place of Buddha. Yesterday's Kapilavastu, Today's Kapileswar" (PDF), The Orissa Historical Research Journal, Orissa State museum, 47 (1), archived from the original (PDF) on 18 March 2012
Tuladhar, Swoyambhu D. (November 2002), "The Ancient City of Kapilvastu – Revisited" (PDF), Ancient Nepal (151): 1–7
Turpie, D (2001), Wesak And The Re-Creation of Buddhist Tradition (PDF) (master's thesis), Montreal, QC: McGill University, archived from the original (PDF) on 15 April 2007
Twitchett, Denis, ed. (1986), The Cambridge History of China, Vol. 1. The Ch'in and Han Empires, 221 BC – AD 220, Cambridge University Press, ISBN 978-0-521-24327-8
Upadhyaya, KN (1971), Early Buddhism and the Bhagavadgita, Delhi: Motilal Banarsidass, p. 95, ISBN 978-81-208-0880-5
Vetter, Tilmann (1988), The Ideas and Meditative Practices of Early Buddhism, Brill
von Hinüber, Oskar (2008). "Hoary past and hazy memory. On the history of early Buddhist texts". Journal of the International Association of Buddhist Studies. 29 (2): 193–210.
Waley, Arthur (July 1932), "Did Buddha die of eating pork?: with a note on Buddha's image", Melanges Chinois et Bouddhiques: 1931–1932, NTU: 343–354, archived from the original on 3 June 2011
Walshe, Maurice (1995), The Long Discourses of the Buddha. A Translation of the Digha Nikaya, Boston: Wisdom Publications
Warder, A.K. (1998). "Lokayata, Ajivaka, and Ajnana Philosophy". A Course in Indian Philosophy (2nd ed.). Delhi: Motilal Banarsidass Publishers. ISBN 978-81-208-1244-4.
——— (2000), Indian Buddhism, Buddhism Series (3rd ed.), Delhi: Motilal Banarsidass
——— (2004). Indian Buddhism (reprint ed.). Delhi: Motilal Banarsidass. Retrieved 13 October 2020.
Wayman, Alex (1971), "Buddhist Dependent Origination", History of Religions, 10 (3): 185–203, doi:10.1086/462628, JSTOR 1062009, S2CID 161507469
Wayman, Alex (1984a), Dependent Origination - the Indo-Tibetan Vision in Wayman (1984)
Wayman, Alex (1984b), The Intermediate-State Dispute in Buddhism in Wayman (1984)
Wayman, Alex (1984), George R. Elder (ed.), Budddhist Insight: Essays by Alex Wayman, Motilall Banarsidass, ISBN 978-81-208-0675-7
Wayman, Alex (1997), Untying the Knots in Buddhism: Selected Essays, Motilal Banarsidass, ISBN 978-81-208-1321-2
Weise, Kai (2013), The Sacred Garden of Lumbini – Perceptions of Buddha's Birthplace (PDF), Paris: UNESCO, ISBN 978-92-3-001208-3, archived from the original (PDF) on 30 August 2014
Willemen, Charles, transl. (2009), Buddhacarita: In Praise of Buddha's Acts (PDF), Berkeley, CA: Numata Center for Buddhist Translation and Research, ISBN 978-1-886439-42-9, archived from the original (PDF) on 27 August 2014
Williams, Paul (2002). Buddhist Thought. Routledge. ISBN 978-0-415-20701-0.
Wynne, Alexander (2004), The Origin of Buddhist Meditation, Routledge
——— (2007), The Origin of Buddhist Meditation (PDF), Routledge, ISBN 978-0-203-96300-5
Yusuf, Imitiyaz (2009). "Dialogue Between Islam and Buddhism through the Concepts Ummatan Wasaṭan (The Middle Nation) and Majjhima-Patipada (The Middle Way)". Islamic Studies. 48 (3): 367–394. ISSN 0578-8072. JSTOR 20839172.
Further reading
Bareau, André (1975), "Les récits canoniques des funérailles du Buddha et leurs anomalies: nouvel essai d'interprétation" [The canonical accounts of the Buddha's funerals and their anomalies: new interpretative essay], Bulletin de l'École Française d'Extrême-Orient (in French), Persée, LXII: 151–189, doi:10.3406/befeo.1975.3845
——— (1979), "La composition et les étapes de la formation progressive du Mahaparinirvanasutra ancien" [The composition and the etapes of the progressive formation of the ancient Mahaparinirvanasutra], Bulletin de l'École Française d'Extrême-Orient (in French), Persée, LXVI: 45–103, doi:10.3406/befeo.1979.4010
Eade, J.C. (1995), The Calendrical Systems of Mainland South-East Asia (illustrated ed.), Brill, ISBN 978-90-04-10437-2
Epstein, Ronald (2003), Buddhist Text Translation Society's Buddhism A to Z (illustrated ed.), Burlingame, CA: Buddhist Text Translation Society
Jones, J.J. (1949), The Mahāvastu, Sacred Books of the Buddhists, vol. 1, London: Luzac & Co.
Kala, U. (2006) [1724], Maha Yazawin Gyi (in Burmese), vol. 1 (4th ed.), Yangon: Ya-Pyei, p. 39
Katz, Nathan (1982), Buddhist Images of Human Perfection: The Arahant of the Sutta Piṭaka, Delhi: Motilal Banarsidass
Kinnard, Jacob N. (1 October 2010). The Emergence of Buddhism: Classical Traditions in Contemporary Perspective. Fortress Press. p. ix. ISBN 978-0-8006-9748-8.
Lamotte, Etienne (1988), History of Indian Buddhism: From the Origins to the Saka Era, Université catholique de Louvain, Institut orientaliste
The life of the Buddha and the early history of his order, derived from Tibetan works in the Bkah-Hgyur and Bstan-Hgyur, followed by notices on the early history of Tibet and Khoten, translated by Rockhill, William Woodville, London: Trübner, 1884
Shimoda, Masahiro (2002), "How has the Lotus Sutra Created Social Movements: The Relationship of the Lotus Sutra to the Mahāparinirvāṇa-sūtra", in Reeves, Gene (ed.), A Buddhist Kaleidoscope, Kosei
Singh, Upinder (2016), A History of Ancient and Early Medieval India: From the Stone Age to the 12th Century, Pearson, ISBN 978-81-317-1677-9
Smith, Donald Eugene (2015). South Asian Politics and Religion. Princeton University Press. ISBN 978-1-4008-7908-3.
Smith, Peter (2000), "Manifestations of God", A concise encyclopaedia of the Bahá'í Faith, Oxford: Oneworld Publications, ISBN 978-1-85168-184-6
von Hinüber, Oskar (2009). "Cremated like a King: The funeral of the Buddha within the ancient Indian context". Journal of the International College of Postgraduate Buddhist Studies. 13: 33–66.
The Buddha

Bechert, Heinz, ed. (1996). When Did the Buddha Live? The Controversy on the Dating of the Historical Buddha. Delhi: Sri Satguru.
Ñāṇamoli Bhikku (1992). The Life of the Buddha According to the Pali Canon (3rd ed.). Kandy, Sri Lanka: Buddhist Publication Society.
Wagle, Narendra K (1995). Society at the Time of the Buddha (2nd ed.). Popular Prakashan. ISBN 978-81-7154-553-7.
Weise, Kai (2013). The Sacred Garden of Lumbini: Perceptions of Buddha's birthplace. UNESCO. ISBN 978-92-3-001208-3.
Early Buddhism

Rahula, Walpola (1974). What the Buddha Taught (2nd ed.). New York: Grove Press.
Vetter, Tilmann (1988), The Ideas and Meditative Practices of Early Buddhism, Brill
Buddhism general

Kalupahana, David J. (1994), A history of Buddhist philosophy, Delhi: Motilal Banarsidass
Robinson, Richard H.; Johnson, Willard L; Wawrytko, Sandra A; DeGraff, Geoffrey (1996). The Buddhist Religion: A Historical Introduction. Belmont, CA: Wadsworth. ISBN 978-0-534-20718-2.
External links

Wikimedia Commons has media related to Gautama Buddha.

Wikiquote has quotations related to The Buddha.

Wikisource has original works by or about:
Gautama Buddha
Works by Buddha at Project Gutenberg
Works by or about Buddha at Internet Archive
Works by or about Siddhārtha Gautama at Internet Archive
Works by or about Shakyamuni at Internet Archive
Buddha on In Our Time at the BBC
A sketch of the Buddha's Life
What Was The Buddha Like? by Ven S. Dhammika
Who was the Buddha? Buddhism for Beginners
Buddhist titles
Preceded by
Kassapa Buddha
Buddhist Patriarch	Succeeded by
Maitreya Buddha
Regnal titles
Preceded by
Krishna
Dashavatara
Kali Yuga	Succeeded by
Kalki
vte
28 Buddhas
TaṇhaṅkaraMedhaṃkaraŚaraṇaṃkaraDīpaṃkaraKauṇḍinyaMaṃgalaSumanasRaivataŚobhitaAnavamadarśinPadmaNāradaPadmottaraSumedhaSujātaPriyadarśinArthadarśinDharmadarśinSiddhārthaTissa BuddhaPuṣyaVipaśyinŚikhinViśvabhūKrakucchandaKanakamuniKāśyapaThe Buddha (Gautama)
Future: Maitreya
icon Religion portal
vte
Topics in Buddhism
vte
The Buddha (Gautama Buddha)
Authority control Edit this at Wikidata
Categories: 5th-century BC Indian people5th century BC in religion5th-century BC philosophers6th-century BC Indian people6th-century BC Indian philosophersAvatars of VishnuBuddhasClassical humanistsDeified peopleFounders of philosophical traditionsFounders of religionsGautama BuddhaIndian ethicistsIndian political philosophersMiracle workersMoral philosophersNational heroes of NepalPhilosophers of ethics and moralityPhilosophers of lovePhilosophers of mindSimple living advocatesSocial philosophersJourney to the West characters
Navigation menu
Not logged in
Talk
Contributions
Create account
Log in
ArticleTalk
ReadView sourceView history
Search
Search Wikipedia
Main page
Contents
Current events
Random article
About Wikipedia
Contact us
Donate
Contribute
Help
Learn to edit
Community portal
Recent changes
Upload file
Tools
What links here
Related changes
Special pages
Permanent link
Page information
Cite this page
Wikidata item
Print/export
Download as PDF
Printable version
In other projects
Wikimedia Commons
Wikiquote
Wikisource

Languages
العربية
डोटेली
Esperanto
한국어
Magyar
日本語
पालि
संस्कृतम्
中文
185 more
Edit links
This page was last edited on 12 November 2022, at 02:46 (UTC).
Text is available under the Creative Commons Attribution-ShareAlike License 3.0; additional terms may apply. By using this site, you agree to the Terms of Use and Privacy Policy. Wikipedia® is a registered trademark of the Wikimedia Foundation, Inc., a non-profit organization.
Privacy policyAbout WikipediaDisclaimersContact WikipediaMobile viewDevelopersStatisticsCookie statementWikimedia FoundationPowered by MediaWiki
"""
    )]
    public void MixTest(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = new byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.InRange(byteCount, 0, value.Length << 1);
        bytes = bytes[..byteCount];
        Span<char> chars = new char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    [InlineData("abcdefg")]
    public void LessThan8AllAsciiTest(string value)
    {
        Assert.InRange(value.Length, 0, 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length + 2, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうえ")]
    [InlineData("あいうえお")]
    [InlineData("あいうえおか")]
    [InlineData("あいうえおかき")]
    public void LessThan8NotAsciiTest(string value)
    {
        Assert.True(value.Length < 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("あ")]
    [InlineData("あい")]
    [InlineData("あいう")]
    public void LessThan4Test(string value)
    {
        Assert.True(value.Length < 4);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }
}