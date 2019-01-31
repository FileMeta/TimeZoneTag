using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileMeta
{
    class Program
    {
        // Test cases
        static readonly TestCase[] s_testCases = new TestCase[]
        {
            new TestCase("0", "0", TimeZoneKind.ForceLocal, 0),
            new TestCase("Z", "Z", TimeZoneKind.ForceUtc, 0),
            new TestCase("+0", "+00:00", TimeZoneKind.Normal, 0),
            new TestCase("-0", "+00:00", TimeZoneKind.Normal, 0),
            new TestCase("+00", "+00:00", TimeZoneKind.Normal, 0),
            new TestCase("-00", "+00:00", TimeZoneKind.Normal, 0),
            new TestCase("+08", "+08:00", TimeZoneKind.Normal, 8*60),
            new TestCase("+3", "+03:00", TimeZoneKind.Normal, 3*60),
            new TestCase("-07", "-07:00", TimeZoneKind.Normal, -7*60),
            new TestCase("-5", "-05:00", TimeZoneKind.Normal, -5*60),
            new TestCase("+4:00", "+04:00", TimeZoneKind.Normal, 4*60),
            new TestCase("-9:00", "-09:00", TimeZoneKind.Normal, -9*60),
            new TestCase("+13:30", "+13:30", TimeZoneKind.Normal, 13*60+30),
            new TestCase("-13:15", "-13:15", TimeZoneKind.Normal, -13*60-15),
            new TestCase("+13:59", "+13:59", TimeZoneKind.Normal, 13*60+59),
            new TestCase("-13:59", "-13:59", TimeZoneKind.Normal, -13*60-59),
            new TestCase("+14:00", "+14:00", TimeZoneKind.Normal, 14*60),
            new TestCase("-14:00", "-14:00", TimeZoneKind.Normal, -14*60),
            new TestCase("+14", "+14:00", TimeZoneKind.Normal, 14*60),
            new TestCase("-14", "-14:00", TimeZoneKind.Normal, -14*60)
        };

        // Parse failure test cases
        static readonly string[] s_parseFailureCases = new string[]
        {
            string.Empty,
            "1",
            "10",
            "+15",
            "-15",
            "+14:01",
            "-14:01",
            "+0:60",
            "+100",
            "-4:60",
            "-222",
            "+04:30:21",
            "Car",
            "A"
        };

        static void Main(string[] args)
        {
            try
            {
                foreach (var testCase in s_testCases)
                {
                    Console.WriteLine(testCase.ToString());
                    testCase.PerformTest();
                }

                Console.WriteLine();
                Console.WriteLine("Parse failure cases:");
                foreach(var testCase in s_parseFailureCases)
                {
                    Console.WriteLine(testCase);
                    TimeZoneTag tag;
                    if (TimeZoneTag.TryParse(testCase, out tag))
                    {
                        throw new ApplicationException("Failed parse failure test");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("All tests passed.");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }

    class TestCase
    {
        const long c_ticksPerMinute = 60L * 10000000L;

        string m_srcTag;
        string m_normalizedTag;
        TimeZoneKind m_kind;
        int m_utcOffset;

        public TestCase(string srcTag, string normalizedTag, TimeZoneKind kind, int utcOffset)
        {
            m_srcTag = srcTag;
            m_normalizedTag = normalizedTag;
            m_kind = kind;
            m_utcOffset = utcOffset;
        }

        // Throws an exception if any test fails.
        // Exceptions are convenient indicators because they can include a message and the location of the error.
        public void PerformTest()
        {
            // Parse test
            TimeZoneTag tag;
            if (!TimeZoneTag.TryParse(m_srcTag, out tag))
            {
                throw new ApplicationException("Failed TryParse test.");
            }

            if (!tag.ToString().Equals(m_normalizedTag, StringComparison.Ordinal))
            {
                throw new ApplicationException("Failed ToString test.");
            }

            if (tag.Kind != m_kind)
            {
                throw new ApplicationException("Failed Kind test.");
            }

            int utcOffset = (tag.Kind == TimeZoneKind.Normal) ? m_utcOffset : 0;

            if (tag.UtcOffset != new TimeSpan(0, utcOffset, 0))
            {
                throw new ApplicationException("Failed UtcOffset test.");
            }

            if (tag.UtcOffsetMinutes != utcOffset)
            {
                throw new ApplicationException("Failed UtcOffsetMinutes test.");
            }

            if (tag.UtcOffsetTicks != utcOffset * c_ticksPerMinute)
            {
                throw new ApplicationException("Failed UtcOffsetTicks test.");
            }

            TimeZoneTag tag2 = new TimeZoneTag(m_utcOffset, m_kind);
            if (tag2.GetHashCode() != tag.GetHashCode())
            {
                throw new ApplicationException("Failed GetHashCode test.");
            }

            if (!tag2.Equals(tag))
            {
                throw new ApplicationException("Failed Equals test.");
            }

            tag2 = new TimeZoneTag(new TimeSpan(0, utcOffset, 0), m_kind);
            if (!tag2.Equals(tag))
            {
                throw new ApplicationException("Failed TimeSpan Constructor test");
            }

            tag2 = new TimeZoneTag(utcOffset * c_ticksPerMinute, m_kind);
            if (!tag2.Equals(tag))
            {
                throw new ApplicationException("Failed Ticks Constructor test");
            }

            if (m_kind == TimeZoneKind.Normal)
            {
                tag2 = new TimeZoneTag(m_utcOffset + 1, m_kind);

                if (tag2.GetHashCode() == tag.GetHashCode())
                {
                    throw new ApplicationException("Failed GetHashCode no match test");
                }

                if (tag2.Equals(tag))
                {
                    throw new ApplicationException("Failed Not Equals test");
                }

                tag2 = new TimeZoneTag(m_utcOffset, TimeZoneKind.ForceLocal);
                if (tag2.Equals(tag))
                {
                    throw new ApplicationException("Failed ForceLocal test");
                }

                tag2 = new TimeZoneTag(m_utcOffset, TimeZoneKind.ForceLocal);
                if (tag2.Equals(tag))
                {
                    throw new ApplicationException("Failed ForceUtc test");
                }

                if (utcOffset == 0 && !tag.Equals(TimeZoneTag.Zero))
                {
                    throw new ApplicationException("Failed Zero test");
                }
            }
            else if (m_kind == TimeZoneKind.ForceLocal)
            {
                if (!tag.Equals(TimeZoneTag.ForceLocal))
                {
                    throw new ApplicationException("Failed ForceLocal test");
                }

                if (tag.Equals(TimeZoneTag.ForceUtc))
                {
                    throw new ApplicationException("Failed ForceUtc test");
                }
            }
            else // m_kind == TimeZoneKind.ForceUtc
            {
                if (!tag.Equals(TimeZoneTag.ForceUtc))
                {
                    throw new ApplicationException("Failed ForceUtc test");
                }

                if (tag.Equals(TimeZoneTag.ForceLocal))
                {
                    throw new ApplicationException("Failed ForceLocal test");
                }
            }

            tag2 = TimeZoneTag.Parse(m_srcTag);
            if (!tag2.Equals(tag))
            {
                throw new ApplicationException("Failed Parse test");
            }

            tag2 = TimeZoneTag.Parse(m_normalizedTag);
            if (!tag2.Equals(tag))
            {
                throw new ApplicationException("Failed Parse Normalized test");
            }

            DateTime dtLocal = new DateTime(1968, 7, 23, 8, 24, 46, 22, DateTimeKind.Local);
            DateTime dtUtc = new DateTime(dtLocal.Ticks - (utcOffset * c_ticksPerMinute), DateTimeKind.Utc);
            DateTimeOffset dto = new DateTimeOffset(dtLocal.Ticks, TimeSpan.FromMinutes(utcOffset));

            if (!tag.ToLocal(dtUtc).Equals(dtLocal))
            {
                throw new ApplicationException("Failed ToLocal test");
            }

            if (!tag.ToUtc(dtLocal).Equals(dtUtc))
            {
                throw new ApplicationException("Failed ToUtc test");
            }

            if (!tag.ToLocal(dtLocal).Equals(dtLocal))
            {
                throw new ApplicationException("Failed ToLocal already local test");
            }

            if (!tag.ToUtc(dtUtc).Equals(dtUtc))
            {
                throw new ApplicationException("Failed ToUtc already utc test");
            }

            if (!tag.ToLocal(DateTime.SpecifyKind(dtUtc, DateTimeKind.Unspecified)).Equals(dtLocal))
            {
                throw new ApplicationException("Failed ToLocal Unspecified test");
            }

            if (!tag.ToUtc(DateTime.SpecifyKind(dtLocal, DateTimeKind.Unspecified)).Equals(dtUtc))
            {
                throw new ApplicationException("Failed ToUtc Unspecified test");
            }

            if (!tag.ToDateTimeOffset(dtUtc).Equals(dto))
            {
                throw new ApplicationException("Failed ToDateTimeOffset UTC test");
            }

            if (!tag.ToDateTimeOffset(dtLocal).Equals(dto))
            {
                throw new ApplicationException("Failed ToDateTimeOffset Local test");
            }
        }

        public override string ToString()
        {
            return m_srcTag;
        }
    }
}
