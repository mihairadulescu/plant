using System.Linq;
using NUnit.Framework;
using Plant.Core;
using Plant.Tests.TestBlueprints;
using Plant.Tests.TestModels;

namespace Plant.Tests
{
    [TestFixture]
    public class BasePlantTest
    {
        [Test]
        public void Can_Create_Two_Different_House()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue", SquareFoot = 50 });
            plant.DefinePropertiesOf(new Person { FirstName = "Leo" });

            var house = plant.Create<House>();
            var redHouse = plant.Create(new House { Color = "red" });

            Assert.AreNotEqual(house, redHouse);
            Assert.AreNotEqual(house.Color, redHouse.Color);
        }

        [Test]
        public void Is_Event_Created_Called()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue", SquareFoot = 50 });
            plant.DefinePropertiesOf(new Person { FirstName = "Leo" });

            plant.BluePrintCreated += plant_BluePrintCreated;
            plant.Create<House>();
            plant.Create<Person>();
        }

        [Test]
        public void Should_Prefill_Relation()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue", SquareFoot = 50 });
            plant.DefinePropertiesOf(new Person { FirstName = "Leo" });

            var house = plant.Create<House>();
            var person = plant.Create<Person>();

            Assert.IsNotNull(person.HouseWhereILive);
            Assert.AreEqual(house, person.HouseWhereILive);
            Assert.AreEqual(house.Color, person.HouseWhereILive.Color);
            Assert.AreEqual(house.SquareFoot, person.HouseWhereILive.SquareFoot);
        }

        [Test]
        public void Should_Not_Prefill_Relation_Defined()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue", SquareFoot = 50 });
            plant.DefinePropertiesOf(new Person { FirstName = "Leo", HouseWhereILive = new House { Color = "Violet" } });

            plant.Create<House>();
            var person = plant.Create<Person>();

            Assert.AreEqual("Violet", person.HouseWhereILive.Color);
        }

        [Test]
        public void Should_Build_Relation()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue", SquareFoot = 50 });
            plant.DefinePropertiesOf(new Person { FirstName = "Leo" });

            var person = plant.Build<Person>();

            Assert.IsNotNull(person.HouseWhereILive);
        }

        [Test]
        public void Should_Create_Variation_Of_Specified_Type()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { FirstName = "" });
            plant.DefineVariationOf<Person>("My", new { FirstName = "My" });
            plant.DefineVariationOf<Person>("Her", new { FirstName = "Her" });

            Assert.IsInstanceOf(typeof(Person), plant.Create<Person>());
            Assert.IsInstanceOf(typeof(Person), plant.Create<Person>("My"));
            Assert.IsInstanceOf(typeof(Person), plant.Create<Person>("Her"));
        }

        [Test]
        public void Should_Create_Variation_With_Extension()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf(new House { Color = "blue" }, OnPropertyPopulation);
            plant.DefineVariationOf("My", new House { Color = "My" }, OnPropertyPopulationVariatoion);

            Assert.AreEqual(plant.Create<House>().Persons.First().FirstName, "Pablo");
            Assert.AreEqual(plant.Create<House>("My").Persons.First().FirstName, "Pedro");
        }

        [Test]
        public void Should_Create_Variation_Of_Specified_Type_With_Correct_Data()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { FirstName = "" });
            plant.DefineVariationOf<Person>("My", new { FirstName = "My" });

            var person = plant.Create<Person>("My");
            Assert.AreEqual("My", person.FirstName);
        }

        [Test]
        public void Create_WhenDefaultHasLazyPropertyOverwrittenByVariation_DoesNotInvokeDefaultLazyProperty()
        {
            var plant = new BasePlant();
            const string variationName = "Self";
            var wasDefaultLazyPropertyAssigned = false;
            plant.DefinePropertiesOf<Person>(new
            {
                FirstName = new LazyProperty<string>(() =>
                {
                    wasDefaultLazyPropertyAssigned = true;
                    return "default";
                })
            });
            plant.DefineVariationOf<Person>(variationName, new { FirstName = "overwritten" });

            plant.Create<Person>(variationName);

            Assert.IsFalse(wasDefaultLazyPropertyAssigned);
        }

        [Test]
        public void Create_WhenVariationHasLazyPropertyOverwrittenByUser_DoesNotInvokeVariationLazyProperty()
        {
            var plant = new BasePlant();
            const string variationName = "Self";
            var wasVariantLazyPropertyAssigned = false;
            plant.DefinePropertiesOf<Person>(new { FirstName = "default" });
            plant.DefineVariationOf<Person>(variationName, new
            {
                FirstName = new LazyProperty<string>(() =>
                {
                    wasVariantLazyPropertyAssigned = true;
                    return "overwritten";
                })
            });

            plant.Create<Person>(new { FirstName = "UserDefined" }, variationName);

            Assert.IsFalse(wasVariantLazyPropertyAssigned);
        }

        [Test]
        public void Should_Create_Instance_Of_Specified_Type()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { FirstName = "" });

            Assert.IsInstanceOf(typeof(Person), plant.Create<Person>());
        }

        [Test]
        public void Should_Create_Instance_With_Requested_Properties()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { FirstName = "" });
            Assert.AreEqual("James", plant.Create<Person>(new { FirstName = "James" }).FirstName);
        }

        [Test]
        public void Should_Use_Default_Instance_Values()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf<Person>(new { FirstName = "Barbara" });
            Assert.AreEqual("Barbara", testPlant.Create<Person>().FirstName);
        }

        [Test]
        public void Should_Create_Instance_With_Null_Value()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf<Person>(new { FirstName = "Barbara", LastName = (string)null });
            Assert.IsNull(testPlant.Create<Person>().LastName);
        }

        [Test]
        public void Should_Create_Instance_With_Default_Properties_Specified_By_Instance()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf(new Person { FirstName = "James" });
            Assert.AreEqual("James", testPlant.Create<Person>().FirstName);
        }

        [Test]
        public void Should_Create_Instance_With_Requested_Properties_Specified_By_Instance()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf(new Person { FirstName = "David" });
            Assert.AreEqual("James", testPlant.Create(new Person { FirstName = "James" }).FirstName);
        }

        [Test]
        [ExpectedException(typeof(PropertyNotFoundException))]
        public void Should_Throw_PropertyNotFound_Exception_When_Given_Invalid_Property()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { Foo = "" });
            plant.Create<Person>();
        }

        [Test]
        [ExpectedException(typeof(TypeNotSetupException))]
        public void Should_Throw_TypeNotSetupException_When_Trying_To_Create_Type_That_Is_Not_Setup()
        {
            new BasePlant().Create<Person>(new { FirstName = "Barbara" });
        }

        [Test]
        public void Should_Set_User_Properties_That_Are_Not_Defaulted()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new { FirstName = "Barbara" });
            Assert.AreEqual("Brechtel", plant.Create<Person>(new { LastName = "Brechtel" }).LastName);
        }

        [Test]
        public void Should_Load_Blueprints_From_Assembly()
        {
            var plant = new BasePlant().WithBlueprintsFromAssemblyOf<TestBlueprint>();
            Assert.AreEqual("Elaine", plant.Create<Person>().MiddleName);
        }

        [Test]
        public void Should_Lazily_Evaluate_Delegate_Properties()
        {
            var plant = new BasePlant();
            string lazyMiddleName = null;
            plant.DefinePropertiesOf<Person>(new
            {
                // ReSharper disable once AccessToModifiedClosure
                MiddleName = new LazyProperty<string>(() => lazyMiddleName)
            });

            Assert.AreEqual(null, plant.Create<Person>().MiddleName);
            lazyMiddleName = "Johnny";
            Assert.AreEqual("Johnny", plant.Create<Person>().MiddleName);
        }

        [Test]
        [ExpectedException(typeof(LazyPropertyHasWrongTypeException))]
        public void Should_Throw_LazyPropertyHasWrongTypeException_When_Lazy_Property_Definition_Returns_Wrong_Type()
        {
            var plant = new BasePlant();
            plant.DefinePropertiesOf<Person>(new
            {
                MiddleName = new LazyProperty<int>(() => 5)
            });

            plant.Create<Person>();
        }

        [Test]
        public void Should_Create_Objects_Via_Constructor()
        {
            var testPlant = new BasePlant();
            testPlant.DefineConstructionOf<Car>(new { Make = "Toyota" });
            Assert.AreEqual("Toyota", testPlant.Create<Car>().Make);
        }

        [Test]
        public void Should_Send_Constructor_Arguments_In_Correct_Order()
        {
            var testPlant = new BasePlant();
            testPlant.DefineConstructionOf<Book>(new { Publisher = "Tor", Author = "Robert Jordan" });
            Assert.AreEqual("Tor", testPlant.Create<Book>().Publisher);
            Assert.AreEqual("Robert Jordan", testPlant.Create<Book>().Author);
        }

        [Test]
        public void Should_Override_Default_Constructor_Arguments()
        {
            var testPlant = new BasePlant();
            testPlant.DefineConstructionOf<House>(new { Color = "Red", SquareFoot = 3000 });

            Assert.AreEqual("Blue", testPlant.Create<House>(new { Color = "Blue" }).Color);
        }

        [Test]
        public void Should_Only_Set_Properties_Once()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf<WriteOnceMemoryModule>(new { Value = 5000 });
            Assert.AreEqual(10, testPlant.Create<WriteOnceMemoryModule>(new { Value = 10 }).Value);
        }

        [Test]
        public void Should_Call_AfterBuildCallback_After_Properties_Populated()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf<Person>(new { FirstName = "Angus", LastName = "MacGyver" },
                p => p.FullName = p.FirstName + p.LastName);
            var builtPerson = testPlant.Create<Person>();
            Assert.AreEqual(builtPerson.FullName, "AngusMacGyver");
        }

        [Test]
        public void Should_Call_AfterBuildCallback_AfterConstructor_Population()
        {
            var testPlant = new BasePlant();
            testPlant.DefineConstructionOf<House>(new { Color = "Red", SquareFoot = 3000 },
                h => h.Summary = h.Color + h.SquareFoot);

            Assert.AreEqual("Blue3000", testPlant.Create<House>(new { Color = "Blue" }).Summary);
        }

        [Test]
        public void Should_increment_values_in_a_sequence_with_property_construction()
        {
            var testPlant = new BasePlant();
            testPlant.DefinePropertiesOf<Person>(new
            {
                FirstName = new Sequence<string>(i => "FirstName" + i)
            });
            Assert.AreEqual("FirstName0", testPlant.Create<Person>().FirstName);
            Assert.AreEqual("FirstName1", testPlant.Create<Person>().FirstName);
        }

        [Test]
        public void Should_increment_values_in_a_sequence_with_ctor_construction()
        {
            var testPlant = new BasePlant();
            testPlant.DefineConstructionOf<House>(new
            {
                Color = new Sequence<string>(i => "Color" + i),
                SquareFoot = 10
            });
            Assert.AreEqual("Color0", testPlant.Create<House>().Color);
            Assert.AreEqual("Color1", testPlant.Create<House>().Color);
        }

        private static void plant_BluePrintCreated(object sender, BluePrintEventArgs e)
        {
            Assert.IsNotNull(e.ObjectConstructed);
        }

        private static void OnPropertyPopulation(House h)
        {
            h.Persons.Add(new Person { FirstName = "Pablo" });
        }

        private static void OnPropertyPopulationVariatoion(House h)
        {
            h.Persons.Clear();
            h.Persons.Add(new Person { FirstName = "Pedro" });
        }
    }

    namespace TestBlueprints
    {
        class TestBlueprint : IBlueprint
        {
            public void SetupPlant(BasePlant plant)
            {
                plant.DefinePropertiesOf<Person>(new
                {
                    MiddleName = "Elaine"
                });
            }
        }
    }
}