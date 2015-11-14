using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClrSpy.CliSupport;
using NUnit.Framework;

namespace ClrSpy.UnitTests.CliSupport
{
    [TestFixture]
    public class TabulatorTests
    {
        [Test]
        public void HeaderAlignmentMatchesColumn()
        {
            var tabulator = new Tabulator(
                new Column("A") { Width = 4 },
                new Column("B") { Width = 4, RightAlign = true }
                ) { Defaults = { Padding = 0 } };

            var header = tabulator.GetHeader();
            Assert.That(header, Is.EqualTo("A      B"));
        }

        [Test]
        public void HeaderDoesNotHaveTrailingLineBreak()
        {
            var tabulator = new Tabulator(
                new Column("A") { Width = 4 },
                new Column("B") { Width = 4 }
                ) { Defaults = { Padding = 0 } };

            var header = tabulator.GetHeader();
            Assert.That(header, Is.Not.StringContaining("\n"));
        }
    }
}
