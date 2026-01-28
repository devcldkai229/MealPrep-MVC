using BusinessObjects.Entities;

namespace MealPrep.DAL.Data
{
    public static class MealSeedData
    {
        public static List<Meal> GetAllMeals(DateTime createdAt)
        {
            var meals = new List<Meal>();

            // PROTEIN-RICH MEALS (Muscle Gain) - 20 meals
            meals.AddRange(GetProteinRichMeals(createdAt));

            // LOW-CALORIE MEALS (Fat Loss) - 20 meals
            meals.AddRange(GetLowCalorieMeals(createdAt));

            // BALANCED MEALS (Maintain) - 20 meals
            meals.AddRange(GetBalancedMeals(createdAt));

            // VEGAN MEALS - 10 meals
            meals.AddRange(GetVeganMeals(createdAt));

            // VEGETARIAN MEALS - 10 meals
            meals.AddRange(GetVegetarianMeals(createdAt));

            // LOW-CARB/KETO MEALS - 10 meals
            meals.AddRange(GetLowCarbMeals(createdAt));

            // HALAL MEALS - 5 meals
            meals.AddRange(GetHalalMeals(createdAt));

            // GLUTEN-FREE MEALS - 5 meals
            meals.AddRange(GetGlutenFreeMeals(createdAt));

            return meals;
        }

        private static List<Meal> GetProteinRichMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal
                {
                    Id = 1,
                    Name = "Ức Gà Nướng Mật Ong",
                    Ingredients = "[\"Ức gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Ức gà nướng thơm lừng với sốt mật ong đậm đà, kèm rau củ tươi ngon. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng và tập luyện.",
                    Calories = 320,
                    Protein = 45.5m,
                    Carbs = 18.2m,
                    Fat = 8.8m,
                    BasePrice = 85000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 2,
                    Name = "Cá Hồi Áp Chảo",
                    Ingredients = "[\"Cá hồi\",\"Bơ\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông cải xanh\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá hồi tươi ngon áp chảo với lớp da giòn tan, kèm rau củ hấp. Nguồn Omega-3 dồi dào, tốt cho tim mạch và não bộ.",
                    Calories = 480,
                    Protein = 42.0m,
                    Carbs = 25.0m,
                    Fat = 22.5m,
                    BasePrice = 120000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 3,
                    Name = "Bò Xào Rau Củ",
                    Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Nấm\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt bò mềm xào với rau củ tươi, sốt đậm đà. Món ăn giàu sắt và protein, phù hợp cho người tập gym.",
                    Calories = 420,
                    Protein = 38.0m,
                    Carbs = 22.0m,
                    Fat = 18.5m,
                    BasePrice = 95000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 4,
                    Name = "Ức Gà Sốt Teriyaki",
                    Ingredients = "[\"Ức gà\",\"Nước tương\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Hành tây\",\"Ớt chuông\",\"Dầu mè\",\"Hạt mè\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Ức gà sốt teriyaki đậm đà kiểu Nhật, kèm rau củ xào. Món ăn giàu protein, ít béo, phù hợp cho người tập luyện.",
                    Calories = 380,
                    Protein = 42.0m,
                    Carbs = 28.0m,
                    Fat = 10.5m,
                    BasePrice = 90000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 5,
                    Name = "Cá Ngừ Nướng",
                    Ingredients = "[\"Cá ngừ\",\"Chanh\",\"Tỏi\",\"Thì là\",\"Khoai lang\",\"Bông cải xanh\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá ngừ tươi nướng vừa chín tới, kèm khoai lang và rau củ. Nguồn protein và Omega-3 dồi dào.",
                    Calories = 400,
                    Protein = 48.0m,
                    Carbs = 30.0m,
                    Fat = 12.0m,
                    BasePrice = 105000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 6,
                    Name = "Gà Tây Nướng",
                    Ingredients = "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.",
                    Calories = 450,
                    Protein = 44.0m,
                    Carbs = 45.0m,
                    Fat = 10.0m,
                    BasePrice = 88000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 7,
                    Name = "Thịt Bò Bít Tết",
                    Ingredients = "[\"Thịt bò\",\"Khoai tây nghiền\",\"Bông cải xanh\",\"Cà rốt\",\"Tỏi\",\"Bơ\",\"Muối\",\"Tiêu\",\"Hương thảo\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt bò bít tết mềm ngon, kèm khoai tây nghiền và rau củ. Nguồn protein và sắt cao.",
                    Calories = 520,
                    Protein = 46.0m,
                    Carbs = 35.0m,
                    Fat = 22.0m,
                    BasePrice = 150000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 8,
                    Name = "Ức Gà Nướng Thảo Mộc",
                    Ingredients = "[\"Ức gà\",\"Hương thảo\",\"Thì là\",\"Tỏi\",\"Chanh\",\"Dầu oliu\",\"Khoai lang\",\"Đậu xanh\",\"Muối\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Ức gà nướng với thảo mộc thơm lừng, kèm khoai lang và đậu xanh. Protein cao, ít béo.",
                    Calories = 350,
                    Protein = 43.0m,
                    Carbs = 32.0m,
                    Fat = 7.5m,
                    BasePrice = 82000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 9,
                    Name = "Cá Thu Nướng",
                    Ingredients = "[\"Cá thu\",\"Chanh\",\"Ớt\",\"Tỏi\",\"Gừng\",\"Rau thơm\",\"Khoai tây\",\"Cà chua\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá thu nướng thơm ngon, giàu Omega-3 và protein. Kèm khoai tây và rau củ.",
                    Calories = 380,
                    Protein = 40.0m,
                    Carbs = 28.0m,
                    Fat = 14.0m,
                    BasePrice = 98000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 10,
                    Name = "Thịt Heo Nướng BBQ",
                    Ingredients = "[\"Thịt heo\",\"Sốt BBQ\",\"Hành tây\",\"Ớt chuông\",\"Bắp\",\"Khoai tây\",\"Tỏi\",\"Gia vị nướng\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt heo nướng sốt BBQ đậm đà, kèm rau củ nướng. Protein cao, hương vị đậm đà.",
                    Calories = 480,
                    Protein = 41.0m,
                    Carbs = 38.0m,
                    Fat = 19.0m,
                    BasePrice = 92000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 11,
                    Name = "Gà Nướng Muối Ớt",
                    Ingredients = "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Rau thơm\",\"Gạo lứt\",\"Rau củ\",\"Dầu ăn\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà nướng muối ớt cay nồng, kèm cơm gạo lứt và rau củ. Protein cao, hương vị đậm đà.",
                    Calories = 420,
                    Protein = 39.0m,
                    Carbs = 40.0m,
                    Fat = 12.0m,
                    BasePrice = 88000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 12,
                    Name = "Cá Basa Chiên Giòn",
                    Ingredients = "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Bánh mì\",\"Rau sống\",\"Sốt tartar\",\"Chanh\",\"Dầu ăn\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá basa chiên giòn vàng, kèm rau sống và sốt tartar. Protein cao, giòn ngon.",
                    Calories = 450,
                    Protein = 36.0m,
                    Carbs = 42.0m,
                    Fat = 15.0m,
                    BasePrice = 75000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 13,
                    Name = "Bò Kho",
                    Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gia vị\",\"Bánh mì\",\"Rau thơm\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Bò kho đậm đà, thịt bò mềm ngon với cà rốt và hành tây. Protein và sắt cao.",
                    Calories = 480,
                    Protein = 44.0m,
                    Carbs = 45.0m,
                    Fat = 16.0m,
                    BasePrice = 110000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 14,
                    Name = "Gà Nướng Lá Chanh",
                    Ingredients = "[\"Gà\",\"Lá chanh\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gừng\",\"Nước mắm\",\"Gạo\",\"Rau củ\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà nướng lá chanh thơm lừng, kèm cơm và rau củ. Protein cao, hương vị đặc trưng.",
                    Calories = 440,
                    Protein = 41.0m,
                    Carbs = 38.0m,
                    Fat = 14.0m,
                    BasePrice = 95000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 15,
                    Name = "Cá Chép Hấp Xì Dầu",
                    Ingredients = "[\"Cá chép\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Dầu mè\",\"Gạo\",\"Rau củ\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá chép hấp xì dầu thơm ngon, kèm cơm và rau củ. Protein cao, ít béo.",
                    Calories = 360,
                    Protein = 38.0m,
                    Carbs = 32.0m,
                    Fat = 10.0m,
                    BasePrice = 85000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 16,
                    Name = "Thịt Bò Xào Lăn",
                    Ingredients = "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\",\"Gạo\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt bò xào lăn đậm đà, kèm cơm. Protein cao, hương vị đậm đà.",
                    Calories = 420,
                    Protein = 40.0m,
                    Carbs = 35.0m,
                    Fat = 16.0m,
                    BasePrice = 98000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 17,
                    Name = "Gà Rang Muối",
                    Ingredients = "[\"Gà\",\"Muối\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà rang muối giòn ngon, kèm cơm và rau củ. Protein cao, đơn giản nhưng ngon.",
                    Calories = 400,
                    Protein = 38.0m,
                    Carbs = 36.0m,
                    Fat = 13.0m,
                    BasePrice = 90000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 18,
                    Name = "Cá Hồi Sashimi Bowl",
                    Ingredients = "[\"Cá hồi\",\"Gạo sushi\",\"Rong biển\",\"Dưa chuột\",\"Cà rốt\",\"Sốt teriyaki\",\"Wasabi\",\"Gừng ngâm\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Bowl cá hồi sashimi tươi ngon, kèm gạo sushi và rau củ. Protein và Omega-3 cao.",
                    Calories = 450,
                    Protein = 44.0m,
                    Carbs = 42.0m,
                    Fat = 15.0m,
                    BasePrice = 130000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 19,
                    Name = "Thịt Bò Nướng Lụi",
                    Ingredients = "[\"Thịt bò\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gia vị nướng\",\"Bánh tráng\",\"Rau sống\",\"Nước chấm\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt bò nướng lụi thơm lừng, kèm bánh tráng và rau sống. Protein cao, hương vị đậm đà.",
                    Calories = 380,
                    Protein = 39.0m,
                    Carbs = 28.0m,
                    Fat = 16.0m,
                    BasePrice = 105000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 20,
                    Name = "Gà Nướng Mật Ong Tỏi",
                    Ingredients = "[\"Gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Khoai tây\",\"Rau củ\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà nướng mật ong tỏi ngọt ngào, kèm khoai tây và rau củ. Protein cao, hương vị đặc biệt.",
                    Calories = 410,
                    Protein = 40.0m,
                    Carbs = 38.0m,
                    Fat = 14.0m,
                    BasePrice = 92000,
                    IsActive = true,
                    CreatedAt = createdAt
                }
            };
        }

        private static List<Meal> GetLowCalorieMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal
                {
                    Id = 21,
                    Name = "Salad Gà Nướng",
                    Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua bi\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Phô mai feta\",\"Hạt óc chó\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad tươi ngon với ức gà nướng, rau xanh giòn, phô mai feta và hạt óc chó. Món ăn nhẹ, giàu chất xơ và vitamin.",
                    Calories = 280,
                    Protein = 28.0m,
                    Carbs = 15.0m,
                    Fat = 14.0m,
                    BasePrice = 75000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 22,
                    Name = "Tôm Sốt Cam",
                    Ingredients = "[\"Tôm tươi\",\"Cam\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Dầu oliu\",\"Muối\"]",
                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
                    Description = "Tôm tươi sốt cam chua ngọt, kèm rau củ. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng.",
                    Calories = 250,
                    Protein = 30.0m,
                    Carbs = 20.0m,
                    Fat = 8.0m,
                    BasePrice = 110000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 23,
                    Name = "Salad Cá Ngừ",
                    Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Dầu giấm\",\"Quả bơ\",\"Hạt hướng dương\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad cá ngừ tươi ngon, kèm rau xanh và quả bơ. Protein cao, ít calo, giàu Omega-3.",
                    Calories = 290,
                    Protein = 32.0m,
                    Carbs = 18.0m,
                    Fat = 12.0m,
                    BasePrice = 95000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 24,
                    Name = "Gà Luộc Rau Củ",
                    Ingredients = "[\"Gà\",\"Cà rốt\",\"Bông cải xanh\",\"Đậu que\",\"Khoai tây\",\"Hành tây\",\"Gia vị\",\"Nước dùng\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà luộc mềm ngon, kèm rau củ hấp. Ít calo, giàu protein, phù hợp cho chế độ giảm cân.",
                    Calories = 320,
                    Protein = 35.0m,
                    Carbs = 22.0m,
                    Fat = 9.0m,
                    BasePrice = 78000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 25,
                    Name = "Cá Hấp Gừng",
                    Ingredients = "[\"Cá\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Nước tương\",\"Dầu mè\",\"Rau củ\",\"Gạo lứt\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá hấp gừng thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.",
                    Calories = 300,
                    Protein = 34.0m,
                    Carbs = 28.0m,
                    Fat = 8.5m,
                    BasePrice = 88000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 26,
                    Name = "Salad Quả Bơ Gà",
                    Ingredients = "[\"Ức gà\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu oliu\",\"Chanh\",\"Muối\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad quả bơ và gà tươi ngon, giàu chất béo tốt và protein. Ít calo, bổ dưỡng.",
                    Calories = 270,
                    Protein = 26.0m,
                    Carbs = 16.0m,
                    Fat = 13.0m,
                    BasePrice = 82000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 27,
                    Name = "Tôm Hấp Bia",
                    Ingredients = "[\"Tôm\",\"Bia\",\"Sả\",\"Ớt\",\"Chanh\",\"Muối\",\"Rau củ\",\"Gạo lứt\"]",
                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
                    Description = "Tôm hấp bia thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.",
                    Calories = 240,
                    Protein = 28.0m,
                    Carbs = 22.0m,
                    Fat = 6.0m,
                    BasePrice = 105000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 28,
                    Name = "Gà Nướng Không Da",
                    Ingredients = "[\"Ức gà không da\",\"Gia vị\",\"Chanh\",\"Rau củ hấp\",\"Khoai lang\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà nướng không da ít béo, kèm rau củ và khoai lang. Protein cao, calo thấp.",
                    Calories = 260,
                    Protein = 38.0m,
                    Carbs = 20.0m,
                    Fat = 5.0m,
                    BasePrice = 75000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 29,
                    Name = "Cá Nướng Giấy Bạc",
                    Ingredients = "[\"Cá\",\"Chanh\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá nướng giấy bạc giữ nguyên hương vị, kèm cơm gạo lứt. Ít calo, giàu protein.",
                    Calories = 280,
                    Protein = 32.0m,
                    Carbs = 26.0m,
                    Fat = 7.0m,
                    BasePrice = 90000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 30,
                    Name = "Salad Trứng Luộc",
                    Ingredients = "[\"Trứng\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad trứng luộc tươi ngon, giàu protein. Ít calo, bổ dưỡng.",
                    Calories = 220,
                    Protein = 18.0m,
                    Carbs = 12.0m,
                    Fat = 11.0m,
                    BasePrice = 65000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 31,
                    Name = "Gà Xào Rau Củ",
                    Ingredients = "[\"Ức gà\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà xào rau củ tươi ngon, ít calo. Protein cao, giàu chất xơ.",
                    Calories = 290,
                    Protein = 32.0m,
                    Carbs = 24.0m,
                    Fat = 8.0m,
                    BasePrice = 80000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 32,
                    Name = "Cá Hồi Nướng Rau Củ",
                    Ingredients = "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Khoai lang\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá hồi nướng kèm rau củ, giàu Omega-3. Ít calo, bổ dưỡng.",
                    Calories = 310,
                    Protein = 36.0m,
                    Carbs = 28.0m,
                    Fat = 10.0m,
                    BasePrice = 115000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 33,
                    Name = "Salad Tôm Bơ",
                    Ingredients = "[\"Tôm\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Chanh\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad tôm và bơ tươi ngon, giàu protein và chất béo tốt. Ít calo.",
                    Calories = 260,
                    Protein = 24.0m,
                    Carbs = 14.0m,
                    Fat = 12.0m,
                    BasePrice = 98000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 34,
                    Name = "Gà Luộc Chấm Muối Tiêu",
                    Ingredients = "[\"Gà\",\"Muối\",\"Tiêu\",\"Chanh\",\"Rau củ hấp\",\"Gạo lứt\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà luộc mềm ngon chấm muối tiêu, kèm rau củ. Ít calo, giàu protein.",
                    Calories = 300,
                    Protein = 34.0m,
                    Carbs = 26.0m,
                    Fat = 8.5m,
                    BasePrice = 78000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 35,
                    Name = "Cá Kho Tộ",
                    Ingredients = "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá kho tộ đậm đà, kèm cơm và rau củ. Ít calo, giàu protein.",
                    Calories = 280,
                    Protein = 30.0m,
                    Carbs = 30.0m,
                    Fat = 6.0m,
                    BasePrice = 85000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 36,
                    Name = "Salad Ức Gà Nướng",
                    Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Hạt hướng dương\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad ức gà nướng tươi ngon, giàu protein. Ít calo, bổ dưỡng.",
                    Calories = 250,
                    Protein = 30.0m,
                    Carbs = 16.0m,
                    Fat = 9.0m,
                    BasePrice = 72000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 37,
                    Name = "Tôm Rang Me",
                    Ingredients = "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]",
                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
                    Description = "Tôm rang me chua ngọt, kèm cơm. Ít calo, giàu protein.",
                    Calories = 270,
                    Protein = 26.0m,
                    Carbs = 32.0m,
                    Fat = 7.0m,
                    BasePrice = 102000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 38,
                    Name = "Gà Hấp Muối",
                    Ingredients = "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Rau củ\",\"Gạo lứt\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà hấp muối mềm ngon, kèm rau củ. Ít calo, giàu protein.",
                    Calories = 290,
                    Protein = 33.0m,
                    Carbs = 24.0m,
                    Fat = 8.0m,
                    BasePrice = 76000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 39,
                    Name = "Cá Nướng Muối Ớt",
                    Ingredients = "[\"Cá\",\"Muối ớt\",\"Chanh\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá nướng muối ớt cay nồng, kèm rau củ. Ít calo, giàu protein.",
                    Calories = 260,
                    Protein = 28.0m,
                    Carbs = 22.0m,
                    Fat = 7.5m,
                    BasePrice = 88000,
                    IsActive = true,
                    CreatedAt = createdAt
                },
                new Meal
                {
                    Id = 40,
                    Name = "Salad Cá Ngừ Đóng Hộp",
                    Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Trứng luộc\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad cá ngừ đóng hộp tiện lợi, giàu protein. Ít calo, bổ dưỡng.",
                    Calories = 240,
                    Protein = 22.0m,
                    Carbs = 14.0m,
                    Fat = 10.0m,
                    BasePrice = 68000,
                    IsActive = true,
                    CreatedAt = createdAt
                }
            };
        }

        private static List<Meal> GetBalancedMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 41, Name = "Cơm Gà Tây Nướng", Ingredients = "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.", Calories = 420, Protein = 40.0m, Carbs = 45.0m, Fat = 10.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 42, Name = "Bowl Quinoa Gà", Ingredients = "[\"Quinoa\",\"Ức gà\",\"Bơ\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Rau mầm\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa đầy đủ dinh dưỡng với ức gà, rau củ tươi và sốt tahini. Món ăn healthy, giàu protein và chất xơ.", Calories = 400, Protein = 35.0m, Carbs = 42.0m, Fat = 12.5m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 43, Name = "Cơm Thịt Kho Tàu", Ingredients = "[\"Thịt ba chỉ\",\"Trứng\",\"Nước dừa\",\"Nước mắm\",\"Đường\",\"Hành tây\",\"Gạo\",\"Dưa chua\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt kho tàu đậm đà, kèm trứng và cơm. Món ăn cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 480, Protein = 32.0m, Carbs = 50.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 44, Name = "Cơm Sườn Nướng", Ingredients = "[\"Sườn heo\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Sườn heo nướng mật ong thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng.", Calories = 520, Protein = 35.0m, Carbs = 48.0m, Fat = 22.0m, BasePrice = 98000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 45, Name = "Cơm Gà Nướng Muối Ớt", Ingredients = "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng muối ớt cay nồng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 450, Protein = 38.0m, Carbs = 42.0m, Fat = 16.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 46, Name = "Cơm Cá Kho Tộ", Ingredients = "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá kho tộ đậm đà, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị truyền thống.", Calories = 380, Protein = 32.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 47, Name = "Cơm Thịt Bò Xào", Ingredients = "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Gạo\",\"Dầu mè\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào đậm đà, kèm cơm. Cân bằng dinh dưỡng, giàu protein.", Calories = 440, Protein = 36.0m, Carbs = 38.0m, Fat = 18.0m, BasePrice = 102000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 48, Name = "Cơm Gà Luộc", Ingredients = "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà luộc mềm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, đơn giản nhưng ngon.", Calories = 400, Protein = 35.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 80000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 49, Name = "Cơm Tôm Rang Me", Ingredients = "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", Description = "Tôm rang me chua ngọt, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc biệt.", Calories = 420, Protein = 28.0m, Carbs = 45.0m, Fat = 14.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 50, Name = "Cơm Cá Chiên", Ingredients = "[\"Cá\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Chanh\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá chiên giòn vàng, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", Calories = 460, Protein = 30.0m, Carbs = 48.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 51, Name = "Cơm Thịt Heo Quay", Ingredients = "[\"Thịt heo quay\",\"Gạo\",\"Dưa chua\",\"Rau củ\",\"Nước mắm\",\"Tỏi\",\"Ớt\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt heo quay giòn tan, kèm cơm và dưa chua. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 500, Protein = 34.0m, Carbs = 46.0m, Fat = 22.0m, BasePrice = 95000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 52, Name = "Cơm Gà Xối Mỡ", Ingredients = "[\"Gà\",\"Mỡ\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước mắm\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xối mỡ thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đặc trưng.", Calories = 480, Protein = 36.0m, Carbs = 44.0m, Fat = 20.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 53, Name = "Cơm Cá Lóc Kho Tộ", Ingredients = "[\"Cá lóc\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá lóc kho tộ đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị truyền thống.", Calories = 390, Protein = 34.0m, Carbs = 38.0m, Fat = 13.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 54, Name = "Cơm Thịt Bò Nướng", Ingredients = "[\"Thịt bò\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giàu protein.", Calories = 460, Protein = 40.0m, Carbs = 40.0m, Fat = 18.0m, BasePrice = 115000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 55, Name = "Cơm Gà Rán", Ingredients = "[\"Gà\",\"Bột chiên\",\"Gia vị\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà rán giòn vàng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giòn ngon.", Calories = 520, Protein = 32.0m, Carbs = 50.0m, Fat = 22.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 56, Name = "Cơm Cá Basa Chiên", Ingredients = "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Dầu ăn\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá basa chiên giòn, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", Calories = 470, Protein = 28.0m, Carbs = 46.0m, Fat = 20.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 57, Name = "Cơm Thịt Heo Nướng", Ingredients = "[\"Thịt heo\",\"Gia vị nướng\",\"Mật ong\",\"Tỏi\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt heo nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", Calories = 490, Protein = 33.0m, Carbs = 44.0m, Fat = 21.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 58, Name = "Cơm Gà Nấu Nấm", Ingredients = "[\"Gà\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nấu nấm thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, bổ dưỡng.", Calories = 410, Protein = 34.0m, Carbs = 42.0m, Fat = 14.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 59, Name = "Cơm Cá Hấp Xì Dầu", Ingredients = "[\"Cá\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Gạo\",\"Rau củ\",\"Dầu mè\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hấp xì dầu thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, ít béo.", Calories = 370, Protein = 32.0m, Carbs = 36.0m, Fat = 11.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 60, Name = "Cơm Thịt Bò Kho", Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gạo\",\"Rau thơm\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò kho đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc trưng.", Calories = 480, Protein = 38.0m, Carbs = 46.0m, Fat = 17.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt }
            };
        }

        private static List<Meal> GetVeganMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 61, Name = "Bowl Quinoa Rau Củ", Ingredients = "[\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với rau củ tươi, sốt tahini. Món ăn vegan healthy, giàu protein thực vật và chất xơ.", Calories = 350, Protein = 12.0m, Carbs = 55.0m, Fat = 10.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 62, Name = "Salad Đậu Hũ Nướng", Ingredients = "[\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad đậu hũ nướng tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", Calories = 280, Protein = 18.0m, Carbs = 20.0m, Fat = 14.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 63, Name = "Cơm Đậu Hũ Chiên", Ingredients = "[\"Đậu hũ\",\"Bột chiên\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ chiên giòn, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", Calories = 380, Protein = 16.0m, Carbs = 52.0m, Fat = 12.0m, BasePrice = 65000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 64, Name = "Bowl Đậu Lăng Rau Củ", Ingredients = "[\"Đậu lăng\",\"Cà rốt\",\"Cần tây\",\"Hành tây\",\"Cà chua\",\"Gia vị\",\"Rau thơm\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu lăng với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", Calories = 320, Protein = 20.0m, Carbs = 48.0m, Fat = 8.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 65, Name = "Salad Tempeh", Ingredients = "[\"Tempeh\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad tempeh tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", Calories = 260, Protein = 22.0m, Carbs = 18.0m, Fat = 12.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 66, Name = "Cơm Đậu Hũ Sốt Cà Chua", Ingredients = "[\"Đậu hũ\",\"Cà chua\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ sốt cà chua đậm đà, kèm cơm. Món ăn vegan, giàu protein thực vật.", Calories = 360, Protein = 15.0m, Carbs = 50.0m, Fat = 10.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 67, Name = "Bowl Đậu Gà Rau Củ", Ingredients = "[\"Đậu gà\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu gà với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", Calories = 340, Protein = 18.0m, Carbs = 46.0m, Fat = 11.0m, BasePrice = 73000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 68, Name = "Salad Quả Bơ Đậu Hũ", Ingredients = "[\"Đậu hũ\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad quả bơ và đậu hũ, giàu chất béo tốt và protein thực vật. Món ăn vegan.", Calories = 300, Protein = 14.0m, Carbs = 22.0m, Fat = 16.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 69, Name = "Cơm Đậu Hũ Nướng", Ingredients = "[\"Đậu hũ\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ nướng thơm lừng, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", Calories = 370, Protein = 17.0m, Carbs = 48.0m, Fat = 13.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 70, Name = "Bowl Seitan Rau Củ", Ingredients = "[\"Seitan\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt teriyaki\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl seitan với rau củ, giàu protein thực vật. Món ăn vegan, hương vị đậm đà.", Calories = 330, Protein = 25.0m, Carbs = 42.0m, Fat = 9.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt }
            };
        }

        private static List<Meal> GetVegetarianMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 71, Name = "Cơm Trứng Chiên", Ingredients = "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Trứng chiên thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", Calories = 380, Protein = 20.0m, Carbs = 45.0m, Fat = 14.0m, BasePrice = 60000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 72, Name = "Cơm Đậu Hũ Sốt Nấm", Ingredients = "[\"Đậu hũ\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước tương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ sốt nấm thơm ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", Calories = 350, Protein = 16.0m, Carbs = 48.0m, Fat = 11.0m, BasePrice = 68000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 73, Name = "Salad Trứng Quả Bơ", Ingredients = "[\"Trứng\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad trứng và quả bơ, giàu protein và chất béo tốt. Món ăn vegetarian.", Calories = 320, Protein = 16.0m, Carbs = 20.0m, Fat = 18.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 74, Name = "Cơm Phô Mai Nướng", Ingredients = "[\"Phô mai\",\"Gạo\",\"Rau củ\",\"Bơ\",\"Tỏi\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Cơm phô mai nướng thơm lừng, kèm rau củ. Món ăn vegetarian, giàu protein và canxi.", Calories = 420, Protein = 18.0m, Carbs = 50.0m, Fat = 16.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 75, Name = "Bowl Trứng Quinoa", Ingredients = "[\"Trứng\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl trứng và quinoa với rau củ, giàu protein. Món ăn vegetarian bổ dưỡng.", Calories = 360, Protein = 19.0m, Carbs = 44.0m, Fat = 13.0m, BasePrice = 78000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 76, Name = "Cơm Đậu Hũ Xào Rau Củ", Ingredients = "[\"Đậu hũ\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Gạo\",\"Nước tương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ xào rau củ tươi ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", Calories = 340, Protein = 15.0m, Carbs = 46.0m, Fat = 10.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 77, Name = "Salad Trứng Đậu Hũ", Ingredients = "[\"Trứng\",\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Dầu giấm\",\"Hạt hướng dương\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad trứng và đậu hũ, giàu protein. Món ăn vegetarian, ít calo.", Calories = 290, Protein = 20.0m, Carbs = 18.0m, Fat = 15.0m, BasePrice = 72000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 78, Name = "Cơm Trứng Ốp La", Ingredients = "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\",\"Tiêu\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Trứng ốp la thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", Calories = 370, Protein = 19.0m, Carbs = 44.0m, Fat = 15.0m, BasePrice = 62000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 79, Name = "Bowl Đậu Hũ Quinoa", Ingredients = "[\"Đậu hũ\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl đậu hũ và quinoa với rau củ, giàu protein thực vật. Món ăn vegetarian.", Calories = 350, Protein = 17.0m, Carbs = 48.0m, Fat = 12.0m, BasePrice = 75000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 80, Name = "Cơm Đậu Hũ Chiên Xù", Ingredients = "[\"Đậu hũ\",\"Bột chiên xù\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Đậu hũ chiên xù giòn tan, kèm cơm và rau củ. Món ăn vegetarian, giòn ngon.", Calories = 390, Protein = 16.0m, Carbs = 50.0m, Fat = 14.0m, BasePrice = 70000, IsActive = true, CreatedAt = createdAt }
            };
        }

        private static List<Meal> GetLowCarbMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 81, Name = "Salad Gà Keto", Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Quả bơ\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad gà keto, ít carb, giàu protein và chất béo tốt. Phù hợp cho chế độ low-carb/keto.", Calories = 320, Protein = 35.0m, Carbs = 8.0m, Fat = 18.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 82, Name = "Cá Hồi Rau Củ Keto", Ingredients = "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hồi với rau củ keto, ít carb, giàu Omega-3. Phù hợp cho chế độ low-carb/keto.", Calories = 380, Protein = 36.0m, Carbs = 10.0m, Fat = 24.0m, BasePrice = 120000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 83, Name = "Thịt Bò Xào Rau Củ Keto", Ingredients = "[\"Thịt bò\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 350, Protein = 38.0m, Carbs = 12.0m, Fat = 18.0m, BasePrice = 98000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 84, Name = "Gà Nướng Rau Củ Keto", Ingredients = "[\"Ức gà\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 330, Protein = 40.0m, Carbs = 9.0m, Fat = 16.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 85, Name = "Salad Cá Ngừ Keto", Ingredients = "[\"Cá ngừ\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", Description = "Salad cá ngừ keto, ít carb, giàu protein và Omega-3. Phù hợp cho chế độ low-carb/keto.", Calories = 300, Protein = 32.0m, Carbs = 7.0m, Fat = 16.0m, BasePrice = 95000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 86, Name = "Thịt Bò Nướng Rau Củ Keto", Ingredients = "[\"Thịt bò\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 360, Protein = 39.0m, Carbs = 11.0m, Fat = 19.0m, BasePrice = 105000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 87, Name = "Cá Hấp Rau Củ Keto", Ingredients = "[\"Cá\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gừng\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cá hấp với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 310, Protein = 34.0m, Carbs = 8.0m, Fat = 15.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 88, Name = "Gà Xào Rau Củ Keto", Ingredients = "[\"Ức gà\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 320, Protein = 36.0m, Carbs = 10.0m, Fat = 14.0m, BasePrice = 82000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 89, Name = "Salad Tôm Keto", Ingredients = "[\"Tôm\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", Description = "Salad tôm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 280, Protein = 28.0m, Carbs = 6.0m, Fat = 14.0m, BasePrice = 110000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 90, Name = "Thịt Bò Xào Nấm Keto", Ingredients = "[\"Thịt bò\",\"Nấm\",\"Bông cải xanh\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Thịt bò xào nấm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", Calories = 340, Protein = 37.0m, Carbs = 9.0m, Fat = 17.0m, BasePrice = 100000, IsActive = true, CreatedAt = createdAt }
            };
        }

        private static List<Meal> GetHalalMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 91, Name = "Cơm Gà Halal", Ingredients = "[\"Gà halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Cơm gà halal thơm ngon, kèm rau củ. Món ăn halal, giàu protein.", Calories = 420, Protein = 38.0m, Carbs = 44.0m, Fat = 14.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 92, Name = "Cơm Thịt Bò Halal", Ingredients = "[\"Thịt bò halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Tỏi\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm thịt bò halal đậm đà, kèm rau củ. Món ăn halal, giàu protein và sắt.", Calories = 460, Protein = 40.0m, Carbs = 42.0m, Fat = 18.0m, BasePrice = 115000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 93, Name = "Cơm Gà Nướng Halal", Ingredients = "[\"Gà halal\",\"Gia vị nướng halal\",\"Gạo\",\"Rau củ\",\"Dầu oliu\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà nướng halal thơm lừng, kèm cơm và rau củ. Món ăn halal, giàu protein.", Calories = 440, Protein = 39.0m, Carbs = 40.0m, Fat = 16.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 94, Name = "Cơm Thịt Cừu Halal", Ingredients = "[\"Thịt cừu halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm thịt cừu halal đậm đà, kèm rau củ. Món ăn halal, giàu protein.", Calories = 480, Protein = 42.0m, Carbs = 38.0m, Fat = 22.0m, BasePrice = 125000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 95, Name = "Cơm Gà Xào Halal", Ingredients = "[\"Gà halal\",\"Hành tây\",\"Ớt chuông\",\"Nấm\",\"Gạo\",\"Gia vị halal\",\"Dầu oliu\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Gà xào halal thơm ngon, kèm cơm. Món ăn halal, giàu protein.", Calories = 400, Protein = 36.0m, Carbs = 38.0m, Fat = 15.0m, BasePrice = 85000, IsActive = true, CreatedAt = createdAt }
            };
        }

        private static List<Meal> GetGlutenFreeMeals(DateTime createdAt)
        {
            return new List<Meal>
            {
                new Meal { Id = 96, Name = "Cơm Gạo Lứt Gà Nướng", Ingredients = "[\"Gà\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Chanh\"]", Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", Description = "Cơm gạo lứt với gà nướng, không chứa gluten. Món ăn gluten-free, giàu chất xơ.", Calories = 410, Protein = 37.0m, Carbs = 46.0m, Fat = 13.0m, BasePrice = 88000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 97, Name = "Bowl Quinoa Cá Hồi", Ingredients = "[\"Cá hồi\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với cá hồi, không chứa gluten. Món ăn gluten-free, giàu Omega-3.", Calories = 390, Protein = 34.0m, Carbs = 40.0m, Fat = 16.0m, BasePrice = 118000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 98, Name = "Cơm Gạo Lứt Thịt Bò", Ingredients = "[\"Thịt bò\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Tỏi\"]", Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", Description = "Cơm gạo lứt với thịt bò, không chứa gluten. Món ăn gluten-free, giàu protein.", Calories = 450, Protein = 38.0m, Carbs = 44.0m, Fat = 17.0m, BasePrice = 108000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 99, Name = "Bowl Quinoa Gà", Ingredients = "[\"Gà\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Dầu oliu\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", Description = "Bowl quinoa với gà, không chứa gluten. Món ăn gluten-free, giàu protein và chất xơ.", Calories = 400, Protein = 36.0m, Carbs = 42.0m, Fat = 14.0m, BasePrice = 92000, IsActive = true, CreatedAt = createdAt },
                new Meal { Id = 100, Name = "Cơm Gạo Lứt Cá Nướng", Ingredients = "[\"Cá\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", Description = "Cơm gạo lứt với cá nướng, không chứa gluten. Món ăn gluten-free, giàu protein.", Calories = 380, Protein = 32.0m, Carbs = 40.0m, Fat = 12.0m, BasePrice = 90000, IsActive = true, CreatedAt = createdAt }
            };
        }
    }
}
