using AtomNini;

namespace AtomNiniTest
{
    public class TestIniFile
    {
        [Fact]
        public void Test1()
        {
            IniFile ini = IniFileManager.GetIniFile("./test.ini");
            string ip = ini.GetValue("PLC", "IP");

            Thread.Sleep(10000);

            Assert.False(ip == ini.GetValue("PLC", "IP"));
        }
    }
}