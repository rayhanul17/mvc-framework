using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using System.Text;

namespace Nexora.Infrastructure.Data.SeedData;

public static class BlogPostSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.BlogPosts.AnyAsync())
            return;

        // Get the admin user to set as author
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
            return;

        // Get categories
        var quranCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "quran-tafseer");
        var hadithCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "hadith-sharif");
        var historyCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "islamic-history");
        var fiqhCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "fiqh-masala");
        var duaCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "dua-zikir");
        var namazCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "namaz-ibadat");
        var storyCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "islamic-stories");
        var ramadanCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "ramadan-special");

        var blogPosts = new List<BlogPost>();

        // Quran & Tafseer Posts
        if (quranCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "সূরা ফাতিহার তাফসীর ও শিক্ষা",
                Slug = "surah-fatiha-tafseer-shikkha",
                Summary = "সূরা ফাতিহা কুরআনের সর্বপ্রথম সূরা এবং নামাজের অপরিহার্য অংশ। এই সূরার গভীর অর্থ ও শিক্ষা নিয়ে বিস্তারিত আলোচনা।",
                Content = @"<h2>সূরা ফাতিহার পরিচয়</h2>
<p>সূরা ফাতিহা পবিত্র কুরআনের প্রথম সূরা। এটি 'উম্মুল কুরআন' বা কুরআনের মা নামেও পরিচিত। এই সূরায় মোট ৭টি আয়াত রয়েছে।</p>

<h3>সূরা ফাতিহার আয়াতসমূহ:</h3>
<p><strong>بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ</strong><br/>
বিসমিল্লাহির রাহমানির রাহীম<br/>
অর্থ: পরম করুণাময়, অসীম দয়ালু আল্লাহর নামে শুরু করছি।</p>

<p><strong>الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَ</strong><br/>
আলহামদুলিল্লাহি রাব্বিল আলামীন<br/>
অর্থ: সমস্ত প্রশংসা আল্লাহর জন্য, যিনি বিশ্বজগতের পালনকর্তা।</p>

<p><strong>الرَّحْمَٰنِ الرَّحِيمِ</strong><br/>
আর রাহমানির রাহীম<br/>
অর্থ: যিনি পরম করুণাময়, অসীম দয়ালু।</p>

<p><strong>مَالِكِ يَوْمِ الدِّينِ</strong><br/>
মালিকি ইয়াওমিদ্দীন<br/>
অর্থ: যিনি বিচার দিনের মালিক।</p>

<p><strong>إِيَّاكَ نَعْبُدُ وَإِيَّاكَ نَسْتَعِينُ</strong><br/>
ইয়্যাকা না'বুদু ওয়া ইয়্যাকা নাস্তাঈন<br/>
অর্থ: আমরা একমাত্র তোমারই ইবাদত করি এবং একমাত্র তোমারই সাহায্য চাই।</p>

<p><strong>اهْدِنَا الصِّرَاطَ الْمُسْتَقِيمَ</strong><br/>
ইহদিনাস সিরাতাল মুস্তাকীম<br/>
অর্থ: আমাদেরকে সরল পথ দেখাও।</p>

<p><strong>صِرَاطَ الَّذِينَ أَنْعَمْتَ عَلَيْهِمْ غَيْرِ الْمَغْضُوبِ عَلَيْهِمْ وَلَا الضَّالِّينَ</strong><br/>
সিরাতাল্লাযীনা আন'আমতা আলাইহিম গাইরিল মাগদূবি আলাইহিম ওয়ালাদ দোয়াল্লীন<br/>
অর্থ: তাদের পথ, যাদেরকে তুমি নিয়ামত দিয়েছ। তাদের পথ নয়, যাদের প্রতি তোমার গজব নাযিল হয়েছে এবং যারা পথভ্রষ্ট হয়েছে।</p>

<h3>সূরা ফাতিহার ফযীলত:</h3>
<ul>
<li>রাসূলুল্লাহ (সা:) বলেছেন, 'সূরা ফাতিহা সকল রোগের ঔষধ।'</li>
<li>প্রতি নামাজে সূরা ফাতিহা পাঠ করা ফরজ।</li>
<li>এটি দুআ কবুলের জন্য অত্যন্ত কার্যকর।</li>
</ul>",
                CategoryId = quranCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "কুরআন,তাফসীর,সূরা ফাতিহা,নামাজ",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-30),
                ViewCount = 150,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1609599006353-e629aaabfeae?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });

            blogPosts.Add(new BlogPost
            {
                Title = "আয়াতুল কুরসী: মহান আয়াতের ফযীলত ও তাৎপর্য",
                Slug = "ayatul-kursi-fazilat-tatporjo",
                Summary = "আয়াতুল কুরসী কুরআনের সবচেয়ে মহান আয়াত। এর অর্থ, ফযীলত এবং আমলের পদ্ধতি সম্পর্কে জানুন।",
                Content = @"<h2>আয়াতুল কুরসীর পরিচয়</h2>
<p>আয়াতুল কুরসী সূরা বাকারার ২৫৫ নম্বর আয়াত। এটি কুরআনের সর্বশ্রেষ্ঠ আয়াত হিসেবে পরিচিত।</p>

<h3>আয়াতুল কুরসীর আরবি ও বাংলা:</h3>
<div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
<p style='font-size: 20px; line-height: 1.8;'>
اللَّهُ لَا إِلَٰهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ ۚ لَا تَأْخُذُهُ سِنَةٌ وَلَا نَوْمٌ ۚ لَّهُ مَا فِي السَّمَاوَاتِ وَمَا فِي الْأَرْضِ ۗ مَن ذَا الَّذِي يَشْفَعُ عِندَهُ إِلَّا بِإِذْنِهِ ۚ يَعْلَمُ مَا بَيْنَ أَيْدِيهِمْ وَمَا خَلْفَهُمْ ۖ وَلَا يُحِيطُونَ بِشَيْءٍ مِّنْ عِلْمِهِ إِلَّا بِمَا شَاءَ ۚ وَسِعَ كُرْسِيُّهُ السَّمَاوَاتِ وَالْأَرْضَ ۖ وَلَا يَئُودُهُ حِفْظُهُمَا ۚ وَهُوَ الْعَلِيُّ الْعَظِيمُ
</p>
</div>

<h3>বাংলা অর্থ:</h3>
<p>আল্লাহ, তিনি ছাড়া কোন উপাস্য নেই। তিনি চিরঞ্জীব, সবকিছুর ধারক। তাঁকে তন্দ্রা স্পর্শ করতে পারে না এবং নিদ্রাও নয়। আসমান ও যমীনে যা কিছু রয়েছে, সব তাঁরই। কে আছে এমন, যে সুপারিশ করবে তাঁর কাছে তাঁর অনুমতি ছাড়া? তাদের সামনে কিংবা পিছনে যা কিছু রয়েছে সে সবই তিনি জানেন। তাঁর জ্ঞানসীমা থেকে তারা কোন কিছুকেই পরিবেষ্টিত করতে পারে না, কিন্তু যতটুকু তিনি ইচ্ছা করেন। তাঁর সিংহাসন সমস্ত আসমান ও যমীনকে পরিবেষ্টিত করে আছে। আর সেগুলোকে ধারণ করা তাঁর পক্ষে কঠিন নয়। তিনিই সর্বোচ্চ এবং সর্বাপেক্ষা মহান।</p>

<h3>আয়াতুল কুরসীর ফযীলত:</h3>
<ol>
<li><strong>শয়তান থেকে হেফাজত:</strong> যে ব্যক্তি সকাল-সন্ধ্যা আয়াতুল কুরসী পাঠ করবে, আল্লাহ তাকে শয়তান থেকে হেফাজত করবেন।</li>
<li><strong>জান্নাতের পথ:</strong> রাসূলুল্লাহ (সা:) বলেছেন, 'যে ব্যক্তি প্রতি ফরজ নামাজের পর আয়াতুল কুরসী পাঠ করবে, তার জন্য জান্নাতে প্রবেশের পথে মৃত্যু ছাড়া আর কোন বাধা থাকবে না।'</li>
<li><strong>ঘুমের সময় পাঠ:</strong> রাতে ঘুমানোর আগে পাঠ করলে সকাল পর্যন্ত আল্লাহর হেফাজতে থাকা যায়।</li>
<li><strong>বরকত লাভ:</strong> ঘরে আয়াতুল কুরসী লিখে রাখলে ঘরে বরকত হয় এবং শয়তান প্রবেশ করতে পারে না।</li>
</ol>

<h3>আমলের পদ্ধতি:</h3>
<ul>
<li>প্রতি ফরজ নামাজের পর একবার পাঠ করুন</li>
<li>সকাল-সন্ধ্যায় একবার করে পাঠ করুন</li>
<li>ঘুমানোর আগে একবার পাঠ করুন</li>
<li>বিপদের সময় বারবার পাঠ করুন</li>
</ul>",
                CategoryId = quranCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "আয়াতুল কুরসী,কুরআন,দুআ,হেফাজত",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-25),
                ViewCount = 200,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1542816417-0983c9c9ad53?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            });
        }

        // Hadith Posts
        if (hadithCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "ঈমানের শাখা-প্রশাখা: বুখারী শরীফের আলোকে",
                Slug = "imaner-shakha-proshakha-bukhari",
                Summary = "রাসূলুল্লাহ (সা:) বলেছেন, ঈমানের ৭০টিরও বেশি শাখা রয়েছে। এই হাদীসের বিস্তারিত ব্যাখ্যা।",
                Content = @"<h2>ঈমানের শাখা সম্পর্কে হাদীস</h2>
<p>হযরত আবু হুরায়রা (রা:) থেকে বর্ণিত, রাসূলুল্লাহ (সা:) বলেছেন:</p>

<blockquote style='background-color: #f0f8ff; padding: 15px; border-left: 4px solid #4CAF50;'>
<p><strong>الإِيمَانُ بِضْعٌ وَسَبْعُونَ أَوْ بِضْعٌ وَسِتُّونَ شُعْبَةً، فَأَفْضَلُهَا قَوْلُ لاَ إِلَهَ إِلاَّ اللَّهُ، وَأَدْنَاهَا إِمَاطَةُ الأَذَى عَنِ الطَّرِيقِ، وَالْحَيَاءُ شُعْبَةٌ مِنَ الإِيمَانِ</strong></p>
<p>'ঈমানের ৭০টিরও বেশি (অথবা ৬০টিরও বেশি) শাখা রয়েছে। এর মধ্যে সর্বোত্তম হলো 'লা ইলাহা ইল্লাল্লাহ' বলা, আর সর্বনিম্ন হলো রাস্তা থেকে কষ্টদায়ক বস্তু সরিয়ে ফেলা। আর লজ্জাশীলতাও ঈমানের একটি শাখা।' (বুখারী ও মুসলিম)</p>
</blockquote>

<h3>ঈমানের প্রধান শাখাসমূহ:</h3>

<h4>১. বিশ্বাসগত শাখা (৩০টি):</h4>
<ul>
<li>আল্লাহর প্রতি ঈমান</li>
<li>ফেরেশতাদের প্রতি ঈমান</li>
<li>আসমানী কিতাবসমূহের প্রতি ঈমান</li>
<li>নবী-রাসূলগণের প্রতি ঈমান</li>
<li>আখিরাতের প্রতি ঈমান</li>
<li>তাকদীরের প্রতি ঈমান</li>
<li>পুনরুত্থানের প্রতি ঈমান</li>
<li>জান্নাত-জাহান্নামের প্রতি ঈমান</li>
<li>আল্লাহকে ভালোবাসা</li>
<li>রাসূল (সা:) কে ভালোবাসা</li>
</ul>

<h4>২. মৌখিক শাখা (৭টি):</h4>
<ul>
<li>কালিমা তাইয়্যিবা পাঠ করা</li>
<li>কুরআন তিলাওয়াত করা</li>
<li>ইলম শিক্ষা করা</li>
<li>দুআ করা</li>
<li>যিকির-আযকার করা</li>
<li>ইস্তিগফার করা</li>
<li>মিথ্যা থেকে বিরত থাকা</li>
</ul>

<h4>৩. শারীরিক শাখা (৩৮টি):</h4>
<ul>
<li>পবিত্রতা অর্জন করা</li>
<li>নামাজ কায়েম করা</li>
<li>যাকাত প্রদান করা</li>
<li>রোজা রাখা</li>
<li>হজ্জ পালন করা</li>
<li>ইতিকাফ করা</li>
<li>লাইলাতুল কদর তালাশ করা</li>
<li>জিহাদ করা</li>
<li>আমানত রক্ষা করা</li>
<li>পিতা-মাতার সেবা করা</li>
</ul>

<h3>এই হাদীসের শিক্ষা:</h3>
<ol>
<li><strong>ঈমানের ব্যাপকতা:</strong> ঈমান শুধু বিশ্বাসের বিষয় নয়, বরং জীবনের সকল ক্ষেত্রে এর প্রভাব রয়েছে।</li>
<li><strong>ছোট আমলের গুরুত্ব:</strong> রাস্তা থেকে কষ্টদায়ক বস্তু সরানোর মতো ছোট কাজও ঈমানের অংশ।</li>
<li><strong>লজ্জাশীলতার মর্যাদা:</strong> লজ্জা ঈমানের একটি গুরুত্বপূর্ণ শাখা।</li>
<li><strong>ঈমানের স্তরভেদ:</strong> ঈমানের বিভিন্ন স্তর রয়েছে এবং তা বৃদ্ধি-হ্রাস পায়।</li>
</ol>",
                CategoryId = hadithCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "হাদীস,ঈমান,বুখারী,মুসলিম",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-20),
                ViewCount = 120,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1585036156261-1e2ac87538c4?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            });
        }

        // Islamic History Posts
        if (historyCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "বদর যুদ্ধ: ইসলামের প্রথম মহাবিজয়",
                Slug = "badr-juddho-islamer-prothom-bijoy",
                Summary = "১৭ রমজান, ২য় হিজরীতে সংঘটিত বদর যুদ্ধ ছিল সত্য ও মিথ্যার মধ্যে প্রথম সরাসরি সংঘর্ষ।",
                Content = @"<h2>বদর যুদ্ধের পটভূমি</h2>
<p>হিজরতের দ্বিতীয় বছর, ১৭ রমজান (৬২৪ খ্রিস্টাব্দ) বদর প্রান্তরে ইসলামের ইতিহাসে প্রথম বড় যুদ্ধ সংঘটিত হয়। এই যুদ্ধকে কুরআনে 'ইয়াওমুল ফুরকান' বা সত্য-মিথ্যার পার্থক্যকারী দিন বলা হয়েছে।</p>

<h3>যুদ্ধের কারণ:</h3>
<ul>
<li>মক্কার কুরাইশদের বাণিজ্য কাফেলা রক্ষার প্রচেষ্টা</li>
<li>মুসলমানদের অর্থনৈতিক অবরোধ</li>
<li>মক্কায় মুসলমানদের উপর অত্যাচারের প্রতিশোধ</li>
</ul>

<h3>উভয় পক্ষের শক্তি:</h3>
<table border='1' style='width: 100%; border-collapse: collapse;'>
<tr style='background-color: #f2f2f2;'>
<th style='padding: 10px;'>বিষয়</th>
<th style='padding: 10px;'>মুসলিম বাহিনী</th>
<th style='padding: 10px;'>কুরাইশ বাহিনী</th>
</tr>
<tr>
<td style='padding: 10px;'>সৈন্য সংখ্যা</td>
<td style='padding: 10px;'>৩১৩ জন</td>
<td style='padding: 10px;'>১০০০ জন</td>
</tr>
<tr>
<td style='padding: 10px;'>ঘোড়া</td>
<td style='padding: 10px;'>২টি</td>
<td style='padding: 10px;'>১০০টি</td>
</tr>
<tr>
<td style='padding: 10px;'>উট</td>
<td style='padding: 10px;'>৭০টি</td>
<td style='padding: 10px;'>৭০০টি</td>
</tr>
</table>

<h3>যুদ্ধের ঘটনাপ্রবাহ:</h3>
<ol>
<li><strong>দুআ ও মুনাজাত:</strong> রাসূলুল্লাহ (সা:) সারারাত আল্লাহর কাছে সাহায্য প্রার্থনা করেন।</li>
<li><strong>একক লড়াই:</strong> হামযা (রা:), আলী (রা:) এবং উবায়দা (রা:) তিনজন কাফির বীরকে পরাজিত করেন।</li>
<li><strong>ফেরেশতাদের সাহায্য:</strong> আল্লাহ তায়ালা ফেরেশতা পাঠিয়ে মুসলমানদের সাহায্য করেন।</li>
<li><strong>কুরাইশদের পরাজয়:</strong> আবু জাহল সহ ৭০ জন কাফির নিহত এবং ৭০ জন বন্দী হয়।</li>
</ol>

<h3>বদর যুদ্ধের ফলাফল:</h3>
<ul>
<li>মুসলমানদের মনোবল বৃদ্ধি পায়</li>
<li>ইসলামী রাষ্ট্রের ভিত্তি মজবুত হয়</li>
<li>আরব উপদ্বীপে মুসলমানদের প্রভাব বৃদ্ধি পায়</li>
<li>শহীদ: ১৪ জন মুসলমান (৬ মুহাজির, ৮ আনসার)</li>
</ul>

<h3>কুরআনে বদর যুদ্ধ:</h3>
<p>আল্লাহ তায়ালা বলেন:</p>
<blockquote>
'নিশ্চয়ই আল্লাহ তোমাদেরকে বদরে সাহায্য করেছেন, যখন তোমরা ছিলে দুর্বল।' (সূরা আল-ইমরান: ১২৩)
</blockquote>",
                CategoryId = historyCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "বদর যুদ্ধ,ইসলামিক ইতিহাস,সীরাত",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-15),
                ViewCount = 180,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1564769625905-50e93615e769?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            });
        }

        // Fiqh & Masala Posts
        if (fiqhCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "ওযুর ফরজ, সুন্নত ও মুস্তাহাব সমূহ",
                Slug = "ozur-foroz-sunnat-mustahab",
                Summary = "নামাজের পূর্বশর্ত ওযু। ওযুর ফরজ কয়টি, সুন্নত কী কী এবং কীভাবে সহীহ পদ্ধতিতে ওযু করতে হয় জানুন।",
                Content = @"<h2>ওযুর গুরুত্ব</h2>
<p>ওযু ইবাদতের জন্য পবিত্রতা অর্জনের একটি গুরুত্বপূর্ণ মাধ্যম। রাসূলুল্লাহ (সা:) বলেছেন, 'পবিত্রতা ঈমানের অর্ধেক।'</p>

<h3>ওযুর ফরজ (৪টি):</h3>
<ol>
<li><strong>মুখমণ্ডল ধোয়া:</strong> কপালের চুলের গোড়া থেকে থুতনির নিচ পর্যন্ত এবং এক কান থেকে অপর কান পর্যন্ত।</li>
<li><strong>দুই হাত কনুই সহ ধোয়া:</strong> আঙুলের ডগা থেকে কনুই পর্যন্ত।</li>
<li><strong>মাথার এক চতুর্থাংশ মাসেহ করা:</strong> ভিজা হাত দিয়ে মাথার উপর হাত বুলানো।</li>
<li><strong>দুই পা টাখনু সহ ধোয়া:</strong> পায়ের আঙুল থেকে টাখনু পর্যন্ত।</li>
</ol>

<h3>ওযুর সুন্নত (১৪টি):</h3>
<ol>
<li>নিয়ত করা</li>
<li>বিসমিল্লাহ বলা</li>
<li>দুই হাত কব্জি পর্যন্ত ধোয়া</li>
<li>মিসওয়াক করা</li>
<li>কুলি করা (৩ বার)</li>
<li>নাকে পানি দেওয়া (৩ বার)</li>
<li>সমস্ত মাথা মাসেহ করা</li>
<li>কান মাসেহ করা</li>
<li>আঙুলের খিলাল করা</li>
<li>দাড়ি খিলাল করা</li>
<li>প্রতিটি অঙ্গ তিনবার ধোয়া</li>
<li>ডান দিক থেকে শুরু করা</li>
<li>ধারাবাহিকতা রক্ষা করা</li>
<li>এক অঙ্গ শুকানোর আগে অন্য অঙ্গ ধোয়া</li>
</ol>

<h3>ওযুর মুস্তাহাব:</h3>
<ul>
<li>কিবলামুখী হয়ে ওযু করা</li>
<li>উঁচু স্থানে বসে ওযু করা</li>
<li>নিজ হাতে ওযু করা</li>
<li>ওযুর সময় কথা না বলা</li>
<li>প্রতিটি অঙ্গ ধোয়ার সময় দুআ পড়া</li>
</ul>

<h3>ওযু ভঙ্গের কারণ:</h3>
<ol>
<li>পায়খানা-প্রস্রাবের রাস্তা দিয়ে কিছু বের হওয়া</li>
<li>শরীরের কোন স্থান থেকে রক্ত বা পুঁজ বের হওয়া</li>
<li>মুখ ভরে বমি হওয়া</li>
<li>চিত বা কাত হয়ে ঘুমানো</li>
<li>পাগল বা অচেতন হওয়া</li>
<li>নামাজে উচ্চস্বরে হাসা</li>
</ol>

<h3>ওযুর পরের দুআ:</h3>
<div style='background-color: #f9f9f9; padding: 10px; border-radius: 5px;'>
<p><strong>أَشْهَدُ أَنْ لَا إِلَهَ إِلَّا اللَّهُ وَحْدَهُ لَا شَرِيكَ لَهُ وَأَشْهَدُ أَنَّ مُحَمَّدًا عَبْدُهُ وَرَسُولُهُ</strong></p>
<p>আশহাদু আল লা-ইলাহা ইল্লাল্লাহু ওয়াহদাহু লা-শারীকা লাহু ওয়া আশহাদু আন্না মুহাম্মাদান আবদুহু ওয়া রাসূলুহু।</p>
</div>",
                CategoryId = fiqhCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "ওযু,পবিত্রতা,ফিকহ,নামাজ",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                ViewCount = 95,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1584552892024-833c2c731c09?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });
        }

        // Dua & Zikir Posts
        if (duaCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "সকাল-সন্ধ্যার মাসনূন দুআ ও যিকির",
                Slug = "sokal-sondhyar-masnun-dua-zikir",
                Summary = "রাসূলুল্লাহ (সা:) নিয়মিত সকাল-সন্ধ্যায় যে দুআ ও যিকিরগুলো করতেন সেগুলো জানুন এবং আমল করুন।",
                Content = @"<h2>সকাল-সন্ধ্যার যিকিরের গুরুত্ব</h2>
<p>সকাল ও সন্ধ্যার যিকির মুমিনের জন্য ঢাল স্বরূপ। এগুলো শয়তান, জিন ও সকল অনিষ্ট থেকে হেফাজত করে।</p>

<h3>সকালের যিকির (ফজরের পর):</h3>

<div style='background-color: #f0f8ff; padding: 15px; margin: 10px 0; border-radius: 5px;'>
<h4>১. আয়াতুল কুরসী (১ বার)</h4>
<p>সকালে একবার পাঠ করলে সন্ধ্যা পর্যন্ত আল্লাহর হেফাজতে থাকা যায়।</p>
</div>

<div style='background-color: #f0fff0; padding: 15px; margin: 10px 0; border-radius: 5px;'>
<h4>২. সূরা ইখলাস, ফালাক, নাস (৩ বার করে)</h4>
<p><strong>قُلْ هُوَ اللَّهُ أَحَدٌ...</strong> (সূরা ইখলাস)<br/>
<strong>قُلْ أَعُوذُ بِرَبِّ الْفَلَقِ...</strong> (সূরা ফালাক)<br/>
<strong>قُلْ أَعُوذُ بِرَبِّ النَّاسِ...</strong> (সূরা নাস)</p>
</div>

<div style='background-color: #fff0f5; padding: 15px; margin: 10px 0; border-radius: 5px;'>
<h4>৩. সাইয়্যিদুল ইস্তিগফার (১ বার)</h4>
<p><strong>اللَّهُمَّ أَنْتَ رَبِّي لَا إِلَهَ إِلَّا أَنْتَ خَلَقْتَنِي وَأَنَا عَبْدُكَ وَأَنَا عَلَى عَهْدِكَ وَوَعْدِكَ مَا اسْتَطَعْتُ أَعُوذُ بِكَ مِنْ شَرِّ مَا صَنَعْتُ أَبُوءُ لَكَ بِنِعْمَتِكَ عَلَيَّ وَأَبُوءُ لَكَ بِذَنْبِي فَاغْفِرْ لِي فَإِنَّهُ لَا يَغْفِرُ الذُّنُوبَ إِلَّا أَنْتَ</strong></p>
<p><em>অর্থ:</em> হে আল্লাহ! তুমি আমার রব, তুমি ছাড়া কোন ইলাহ নেই। তুমি আমাকে সৃষ্টি করেছ, আমি তোমার বান্দা...</p>
</div>

<div style='background-color: #f5f5f5; padding: 15px; margin: 10px 0; border-radius: 5px;'>
<h4>৪. সকালের দুআ (১ বার)</h4>
<p><strong>أَصْبَحْنَا وَأَصْبَحَ الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ لَا إِلَهَ إِلَّا اللَّهُ وَحْدَهُ لَا شَرِيكَ لَهُ لَهُ الْمُلْكُ وَلَهُ الْحَمْدُ وَهُوَ عَلَى كُلِّ شَيْءٍ قَدِيرٌ</strong></p>
<p><em>অর্থ:</em> আমরা সকালে উপনীত হয়েছি এবং সমস্ত রাজত্ব আল্লাহর জন্য...</p>
</div>

<h3>অন্যান্য গুরুত্বপূর্ণ যিকির:</h3>
<ul>
<li><strong>সুবহানাল্লাহ (৩৩ বার)</strong> - আল্লাহর পবিত্রতা ঘোষণা</li>
<li><strong>আলহামদুলিল্লাহ (৩৩ বার)</strong> - আল্লাহর প্রশংসা</li>
<li><strong>আল্লাহু আকবার (৩৩ বার)</strong> - আল্লাহর মহত্ত্ব ঘোষণা</li>
<li><strong>লা ইলাহা ইল্লাল্লাহ (১০০ বার)</strong> - তাওহীদের ঘোষণা</li>
</ul>

<h3>সন্ধ্যার যিকির (মাগরিবের পর):</h3>
<p>সকালের যিকিরগুলোই সন্ধ্যায় পুনরায় পাঠ করতে হয়। তবে 'أَصْبَحْنَا' এর স্থলে 'أَمْسَيْنَا' বলতে হয়।</p>

<h3>যিকিরের ফযীলত:</h3>
<ol>
<li>শয়তান থেকে হেফাজত</li>
<li>দুশ্চিন্তা ও অস্থিরতা দূর হয়</li>
<li>রিযিকে বরকত আসে</li>
<li>গুনাহ মাফ হয়</li>
<li>জান্নাতের পথ সুগম হয়</li>
</ol>",
                CategoryId = duaCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "দুআ,যিকির,সকাল,সন্ধ্যা,মাসনূন",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-8),
                ViewCount = 110,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1602525653485-5e85e5474758?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            });
        }

        // Namaz & Ibadat Posts
        if (namazCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "পাঁচ ওয়াক্ত নামাজের ফরজ, ওয়াজিব ও সুন্নত",
                Slug = "panch-waqt-namaz-foroz-wajib-sunnat",
                Summary = "দৈনিক পাঁচ ওয়াক্ত নামাজের ফরজ, ওয়াজিব, সুন্নত রাকাত সংখ্যা এবং সময়সূচী সম্পর্কে বিস্তারিত জানুন।",
                Content = @"<h2>নামাজের গুরুত্ব</h2>
<p>নামাজ ইসলামের দ্বিতীয় স্তম্ভ। আল্লাহ তায়ালা বলেন, 'নিশ্চয়ই নামাজ মুমিনদের উপর নির্দিষ্ট সময়ে ফরজ।' (সূরা নিসা: ১০৩)</p>

<h3>পাঁচ ওয়াক্ত নামাজের রাকাত সংখ্যা:</h3>

<table border='1' style='width: 100%; border-collapse: collapse;'>
<tr style='background-color: #4CAF50; color: white;'>
<th style='padding: 10px;'>ওয়াক্ত</th>
<th style='padding: 10px;'>সুন্নত (কাবলাল)</th>
<th style='padding: 10px;'>ফরজ</th>
<th style='padding: 10px;'>সুন্নত (বা'দাল)</th>
<th style='padding: 10px;'>নফল/ওয়াজিব</th>
<th style='padding: 10px;'>মোট</th>
</tr>
<tr>
<td style='padding: 10px;'><strong>ফজর</strong></td>
<td style='padding: 10px;'>২ (সুন্নতে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>২</td>
<td style='padding: 10px;'>-</td>
<td style='padding: 10px;'>-</td>
<td style='padding: 10px;'>৪</td>
</tr>
<tr style='background-color: #f2f2f2;'>
<td style='padding: 10px;'><strong>যোহর</strong></td>
<td style='padding: 10px;'>৪ (সুন্নতে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>৪</td>
<td style='padding: 10px;'>২ (সুন্নতে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>২ (নফল)</td>
<td style='padding: 10px;'>১২</td>
</tr>
<tr>
<td style='padding: 10px;'><strong>আসর</strong></td>
<td style='padding: 10px;'>৪ (সুন্নতে গায়রে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>৪</td>
<td style='padding: 10px;'>-</td>
<td style='padding: 10px;'>-</td>
<td style='padding: 10px;'>৮</td>
</tr>
<tr style='background-color: #f2f2f2;'>
<td style='padding: 10px;'><strong>মাগরিব</strong></td>
<td style='padding: 10px;'>-</td>
<td style='padding: 10px;'>৩</td>
<td style='padding: 10px;'>২ (সুন্নতে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>২ (নফল)</td>
<td style='padding: 10px;'>৭</td>
</tr>
<tr>
<td style='padding: 10px;'><strong>এশা</strong></td>
<td style='padding: 10px;'>৪ (সুন্নতে গায়রে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>৪</td>
<td style='padding: 10px;'>২ (সুন্নতে মুয়াক্কাদা)</td>
<td style='padding: 10px;'>৩ (বিতর ওয়াজিব) + ২ (নফল)</td>
<td style='padding: 10px;'>১৫</td>
</tr>
</table>

<h3>নামাজের ফরজ (৭টি):</h3>
<ol>
<li>শরীর পাক হওয়া</li>
<li>কাপড় পাক হওয়া</li>
<li>নামাজের জায়গা পাক হওয়া</li>
<li>সতর ঢাকা</li>
<li>কিবলামুখী হওয়া</li>
<li>ওয়াক্ত হওয়া</li>
<li>নিয়ত করা</li>
</ol>

<h3>নামাজের ওয়াজিব (১৪টি):</h3>
<ol>
<li>তাকবীরে তাহরীমার জন্য হাত উঠানো</li>
<li>ছানা পড়া</li>
<li>সূরা ফাতিহা পড়া</li>
<li>সূরা ফাতিহার সাথে অন্য সূরা মিলানো</li>
<li>রুকুতে তাসবীহ পড়া</li>
<li>সিজদায় তাসবীহ পড়া</li>
<li>প্রথম বৈঠকে তাশাহহুদ পড়া</li>
<li>শেষ বৈঠকে তাশাহহুদ পড়া</li>
<li>দুই সিজদার মাঝে সোজা হয়ে বসা</li>
<li>রুকু থেকে সোজা হয়ে দাঁড়ানো</li>
<li>তারতীব ঠিক রাখা</li>
<li>তা'দীলে আরকান (ধীরস্থিরতা)</li>
<li>বিতর নামাজে দুআ কুনূত পড়া</li>
<li>দুই ঈদের নামাজে অতিরিক্ত তাকবীর</li>
</ol>

<h3>নামাজের সময়সূচী:</h3>
<ul>
<li><strong>ফজর:</strong> সুবহে সাদিক থেকে সূর্যোদয়ের পূর্ব পর্যন্ত</li>
<li><strong>যোহর:</strong> সূর্য পশ্চিম দিকে ঢলে পড়া থেকে কোন বস্তুর ছায়া দ্বিগুণ হওয়া পর্যন্ত</li>
<li><strong>আসর:</strong> যোহরের সময় শেষ থেকে সূর্যাস্তের পূর্ব পর্যন্ত</li>
<li><strong>মাগরিব:</strong> সূর্যাস্তের পর থেকে পশ্চিম আকাশের লালিমা দূর হওয়া পর্যন্ত</li>
<li><strong>এশা:</strong> মাগরিবের সময় শেষ থেকে সুবহে সাদিকের পূর্ব পর্যন্ত</li>
</ul>",
                CategoryId = namazCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "নামাজ,ফরজ,সুন্নত,ওয়াজিব,ইবাদত",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-5),
                ViewCount = 250,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1591604021695-0c69b7c05981?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            });
        }

        // Islamic Stories Posts
        if (storyCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "হযরত ইব্রাহীম (আ:) এর ত্যাগ ও কুরবানীর ইতিহাস",
                Slug = "hazrat-ibrahim-tyag-kurbani-itihas",
                Summary = "হযরত ইব্রাহীম (আ:) এর জীবনের সবচেয়ে বড় পরীক্ষা এবং ইসমাইল (আ:) এর কুরবানীর ঘটনা।",
                Content = @"<h2>ইব্রাহীম (আ:) এর পরিচয়</h2>
<p>হযরত ইব্রাহীম (আ:) 'খলীলুল্লাহ' বা আল্লাহর বন্ধু উপাধিতে ভূষিত। তিনি 'আবুল আম্বিয়া' বা নবীদের পিতা নামেও পরিচিত। তাঁর জীবন ছিল ত্যাগ ও কুরবানীর উজ্জ্বল দৃষ্টান্ত।</p>

<h3>স্বপ্নে আদেশ:</h3>
<p>হযরত ইব্রাহীম (আ:) বৃদ্ধ বয়সে আল্লাহর কাছে সন্তান প্রার্থনা করেন। আল্লাহ তাঁকে ইসমাইল (আ:) নামে একটি পুত্র সন্তান দান করেন। যখন ইসমাইল (আ:) কিশোর বয়সে পৌঁছান, তখন ইব্রাহীম (আ:) স্বপ্নে দেখেন যে, তিনি তাঁর প্রিয় পুত্রকে কুরবানী করছেন।</p>

<h3>পিতা-পুত্রের কথোপকথন:</h3>
<blockquote style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid #4CAF50;'>
<p>আল্লাহ তায়ালা কুরআনে বলেন:<br/>
'যখন সে (ইসমাইল) তার সাথে চলাফেরা করার বয়সে পৌঁছল, তখন ইব্রাহীম বলল, হে প্রিয় পুত্র! আমি স্বপ্নে দেখেছি যে, তোমাকে যবেহ করছি; এখন তোমার অভিমত কী? সে বলল, হে আমার পিতা! আপনাকে যা আদেশ করা হয়েছে, আপনি তাই করুন। ইনশাআল্লাহ, আপনি আমাকে ধৈর্যশীলদের মধ্যে পাবেন।' (সূরা সাফফাত: ১০২)</p>
</blockquote>

<h3>মহান ত্যাগের মুহূর্ত:</h3>
<p>পিতা-পুত্র উভয়ে আল্লাহর আদেশ পালনে প্রস্তুত হলেন। ইব্রাহীম (আ:) পুত্রকে কুরবানীর জন্য শুইয়ে দিলেন। কিন্তু আল্লাহ তায়ালা তাঁদের ত্যাগ কবুল করে নিলেন এবং ইসমাইল (আ:) এর পরিবর্তে একটি দুম্বা কুরবানী করার ব্যবস্থা করলেন।</p>

<h3>আল্লাহর পুরস্কার:</h3>
<p>আল্লাহ তায়ালা বলেন:</p>
<blockquote>
'আমি তাকে এক মহান কুরবানীর বিনিময়ে মুক্ত করলাম। আর আমি তাকে পরবর্তীদের মধ্যে স্মরণীয় করে রাখলাম। ইব্রাহীমের উপর শান্তি বর্ষিত হোক।' (সূরা সাফফাত: ১০৭-১০৯)
</blockquote>

<h3>শিক্ষা ও উপদেশ:</h3>
<ol>
<li><strong>আল্লাহর প্রতি নিঃশর্ত আত্মসমর্পণ:</strong> ইব্রাহীম (আ:) আল্লাহর আদেশ পালনে কোন দ্বিধা করেননি।</li>
<li><strong>সন্তানের আনুগত্য:</strong> ইসমাইল (আ:) পিতার আদেশ এবং আল্লাহর হুকুম পালনে সম্মত হন।</li>
<li><strong>ত্যাগের মহিমা:</strong> আল্লাহর জন্য সবচেয়ে প্রিয় বস্তু ত্যাগ করার মানসিকতা।</li>
<li><strong>তাওয়াক্কুল:</strong> আল্লাহর উপর পূর্ণ ভরসা।</li>
</ol>

<h3>কুরবানীর বিধান:</h3>
<p>এই ঘটনার স্মরণে প্রতি বছর জিলহজ্জ মাসের ১০, ১১ ও ১২ তারিখে মুসলমানরা পশু কুরবানী করে থাকেন। এটি সামর্থ্যবান মুসলমানদের জন্য ওয়াজিব।</p>

<h3>কুরবানীর তাৎপর্য:</h3>
<ul>
<li>আল্লাহর নৈকট্য লাভ</li>
<li>ত্যাগের মনোভাব সৃষ্টি</li>
<li>গরীব-দুঃখীদের সাহায্য</li>
<li>ইব্রাহীম (আ:) এর সুন্নত পালন</li>
</ul>",
                CategoryId = storyCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "ইব্রাহীম,কুরবানী,ইসলামিক গল্প,নবী",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-3),
                ViewCount = 175,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            });
        }

        // Ramadan Special Posts
        if (ramadanCategory != null)
        {
            blogPosts.Add(new BlogPost
            {
                Title = "রমজান মাসের ফযীলত ও করণীয়",
                Slug = "ramadan-maser-fazilat-koroniy",
                Summary = "পবিত্র রমজান মাসের ফযীলত, রোজার নিয়ম-কানুন এবং এই মাসে করণীয় আমলসমূহ।",
                Content = @"<h2>রমজানের পরিচয়</h2>
<p>রমজান হিজরী বর্ষের নবম মাস। এই মাসে পবিত্র কুরআন নাযিল হয়েছে এবং এই মাসে রোজা রাখা ফরজ করা হয়েছে।</p>

<h3>রমজানের ফযীলত:</h3>
<blockquote style='background-color: #f0f8ff; padding: 15px; border-left: 4px solid #4CAF50;'>
<p>রাসূলুল্লাহ (সা:) বলেছেন: 'যখন রমজান মাস আসে, তখন জান্নাতের দরজাসমূহ খুলে দেওয়া হয়, জাহান্নামের দরজাসমূহ বন্ধ করে দেওয়া হয় এবং শয়তানদের শৃংখলাবদ্ধ করা হয়।' (বুখারী ও মুসলিম)</p>
</blockquote>

<h3>রোজার ফরজ (২টি):</h3>
<ol>
<li><strong>নিয়ত করা:</strong> সুবহে সাদিকের পূর্বে রোজার নিয়ত করা ফরজ।</li>
<li><strong>বিরত থাকা:</strong> সুবহে সাদিক থেকে সূর্যাস্ত পর্যন্ত পানাহার ও স্ত্রী সহবাস থেকে বিরত থাকা।</li>
</ol>

<h3>রোজা ভঙ্গের কারণ:</h3>
<ul>
<li>ইচ্ছাকৃত পানাহার করা</li>
<li>স্ত্রী সহবাস করা</li>
<li>ইচ্ছাকৃত বমি করা</li>
<li>ইনজেকশন বা স্যালাইনের মাধ্যমে খাদ্য গ্রহণ</li>
<li>নাক বা কানের মাধ্যমে ঔষধ মস্তিষ্কে পৌঁছানো</li>
</ul>

<h3>রমজানের বিশেষ আমল:</h3>

<h4>১. তারাবীহ নামাজ:</h4>
<p>এশার নামাজের পর ২০ রাকাত তারাবীহ নামাজ পড়া সুন্নতে মুয়াক্কাদা। এতে পুরো কুরআন খতম করা হয়।</p>

<h4>২. সাহরী খাওয়া:</h4>
<p>রাসূলুল্লাহ (সা:) বলেছেন, 'তোমরা সাহরী খাও, কেননা সাহরীতে বরকত রয়েছে।' (বুখারী)</p>

<h4>৩. ইফতার করা:</h4>
<p>সূর্যাস্তের সাথে সাথে ইফতার করা সুন্নত। খেজুর দিয়ে ইফতার করা উত্তম।</p>

<h4>৪. কুরআন তিলাওয়াত:</h4>
<p>রমজান কুরআনের মাস। এই মাসে বেশি বেশি কুরআন তিলাওয়াত করা উচিত।</p>

<h4>৫. ইতিকাফ:</h4>
<p>রমজানের শেষ দশ দিন মসজিদে ইতিকাফ করা সুন্নতে মুয়াক্কাদা কিফায়া।</p>

<h4>৬. লাইলাতুল কদর তালাশ:</h4>
<p>রমজানের শেষ দশ দিনের বিজোড় রাতগুলোতে লাইলাতুল কদর তালাশ করা।</p>

<h3>রমজানের দুআ:</h3>
<div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px;'>
<p><strong>ইফতারের দুআ:</strong><br/>
اللَّهُمَّ لَكَ صُمْتُ وَعَلَى رِزْقِكَ أَفْطَرْتُ<br/>
'আল্লাহুম্মা লাকা সুমতু ওয়া আলা রিযকিকা আফতারতু'<br/>
অর্থ: হে আল্লাহ! তোমার জন্য রোজা রেখেছি এবং তোমার রিযিক দিয়ে ইফতার করছি।</p>
</div>

<h3>রমজানের শিক্ষা:</h3>
<ol>
<li>তাকওয়া অর্জন</li>
<li>সবর ও সংযমের অনুশীলন</li>
<li>গরীবদের কষ্ট অনুধাবন</li>
<li>আত্মশুদ্ধি</li>
<li>আল্লাহর নৈকট্য লাভ</li>
</ol>",
                CategoryId = ramadanCategory.Id,
                AuthorId = adminUser.Id,
                Tags = "রমজান,রোজা,তারাবীহ,ইতিকাফ,লাইলাতুল কদর",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-1),
                ViewCount = 300,
                FeaturedImageUrl = "https://images.unsplash.com/photo-1591123120675-6f7f1aae0e5b?w=800&h=400&fit=crop",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });
        }

        await context.BlogPosts.AddRangeAsync(blogPosts);
        await context.SaveChangesAsync();
    }
}