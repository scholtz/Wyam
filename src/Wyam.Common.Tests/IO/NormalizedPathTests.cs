﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests.IO
{
    // TODO: ToString() tests:
    // Provder|Path if absolute and provider contains path information in the URI
    // Provider/Path if absolute and provider does not contain path info (I.e., looks like a URI)
    // Path if absolute and provider is null
    // Path if relative


    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class NormalizedPathTests : BaseFixture
    {
        private class TestPath : NormalizedPath
        {
            public TestPath(string path, PathKind pathKind = PathKind.RelativeOrAbsolute) : base(path, pathKind)
            {
            }

            public TestPath(string provider, string path, PathKind pathKind = PathKind.RelativeOrAbsolute) : base(provider, path, pathKind)
            {
            }

            public TestPath(Uri provider, string path, PathKind pathKind = PathKind.RelativeOrAbsolute) : base(provider, path, pathKind)
            {
            }

            public TestPath(Uri path) : base(path)
            {
            }
        }

        public class ConstructorTests : NormalizedPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(null));
            }

            [Test]
            public void ShouldThrowIfProviderIsSpecifiedForRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath("foo", "Hello/World"));
            }
            
            [TestCase("")]
            [TestCase("\t ")]
            public void ShouldThrowIfPathIsEmpty(string fullPath)
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath(fullPath));
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                TestPath path = new TestPath("./");

                // Then
                Assert.AreEqual(".", path.FullPath);
            }

            [Test]
            public void WillNormalizePathSeparators()
            {
                // Given, When
                TestPath path = new TestPath("shaders\\basic");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void WillTrimWhiteSpaceFromPath()
            {
                // Given, When
                TestPath path = new TestPath(" shaders/basic ");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void WillNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                TestPath path = new TestPath("my awesome shaders/basic");

                // Then
                Assert.AreEqual("my awesome shaders/basic", path.FullPath);
            }
            
            [TestCase("/Hello/World/", "/Hello/World")]
            [TestCase("\\Hello\\World\\", "/Hello/World")]
            [TestCase("file.txt/", "file.txt")]
            [TestCase("file.txt\\", "file.txt")]
            [TestCase("Temp/file.txt/", "Temp/file.txt")]
            [TestCase("Temp\\file.txt\\", "Temp/file.txt")]
            [TestCase("http://www.foo.bar/", "http://www.foo.bar")]
            [TestCase("http://www.foo.bar/test/page.html/", "http://www.foo.bar/test/page.html")]
            public void ShouldRemoveTrailingSlashes(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }
            
            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }
            
            [TestCase("./Hello/World/", "Hello/World")]
            [TestCase(".\\Hello/World/", "Hello/World")]
            [TestCase("./file.txt", "file.txt")]
            [TestCase("./Temp/file.txt", "Temp/file.txt")]
            public void ShouldRemoveRelativePrefix(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }
            
            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }
        }

        public class ProviderPropertyTests : NormalizedPathTests
        {
            [TestCase("foo", "/Hello/World", "foo")]
            [TestCase("", "/Hello/World", "")]
            [TestCase(null, "/Hello/World", "")]
            [TestCase(null, "Hello/World", null)]
            [TestCase("", "Hello/World", null)]
            public void ShouldReturnProvider(string provider, string pathName, string expectedProvider)
            {
                // Given, W
                TestPath path = new TestPath(provider, pathName);

                // Then
                Assert.AreEqual(expectedProvider, path.Provider);

            }
        }

        public class SegmentsPropertyTests : NormalizedPathTests
        {
            [TestCase("Hello/World")]
            [TestCase("/Hello/World")]
            [TestCase("/Hello/World/")]
            [TestCase("./Hello/World/")]
            public void ShouldReturnSegmentsOfPath(string pathName)
            {
                // Given, When
                TestPath path = new TestPath(pathName);

                // Then
                Assert.AreEqual(2, path.Segments.Length);
                Assert.AreEqual("Hello", path.Segments[0]);
                Assert.AreEqual("World", path.Segments[1]);
            }
        }

        public class FullPathPropertyTests : NormalizedPathTests
        {
            [Test]
            public void ShouldReturnFullPath()
            {
                // Given, When
                const string expected = "shaders/basic";
                TestPath path = new TestPath(expected);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }
        }

        public class RootPropertyTests : NormalizedPathTests
        {
            [TestCase(@"\a\b\c", "/")]
            [TestCase("/a/b/c", "/")]
            [TestCase("a/b/c", ".")]
            [TestCase(@"a\b\c", ".")]
            [TestCase("foo.txt", ".")]
            [TestCase("foo", ".")]
#if !UNIX
            [TestCase(@"c:\a\b\c", "c:/")]
            [TestCase("c:/a/b/c", "c:/")]
#endif
            public void ShouldReturnRootPath(string fullPath, string expected)
            {
                // Given
                TestPath path = new TestPath(fullPath);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(expected, root.FullPath);
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
#if !UNIX
            [TestCase(@"c:\a\b\c")]
            [TestCase("c:/a/b/c")]
#endif
            public void ShouldReturnDottedRootForExplicitRelativePath(string fullPath)
            {
                // Given
                TestPath path = new TestPath(fullPath, PathKind.Relative);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(".", root.FullPath);
            }
        }

        public class IsRelativePropertyTests : NormalizedPathTests
        {
            [TestCase("assets/shaders", true)]
            [TestCase("assets/shaders/basic.frag", true)]
            [TestCase("/assets/shaders", false)]
            [TestCase("/assets/shaders/basic.frag", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelative(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }

#if !UNIX
            [TestCase("c:/assets/shaders", false)]
            [TestCase("c:/assets/shaders/basic.frag", false)]
            [TestCase("c:/", false)]
            [TestCase("c:", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelativeOnWindows(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }
#endif
        }
        
        public class ToStringMethodTests : NormalizedPathTests
        {
            [Test]
            public void Should_Return_The_Full_Path()
            {
                // Given, When
                TestPath path = new TestPath("temp/hello");

                // Then
                Assert.AreEqual("temp/hello", path.ToString());
            }
        }

        public class CollapseMethodTests : NormalizedPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When
                TestDelegate test = () => NormalizedPath.Collapse(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
            
            [TestCase("hello/temp/test/../../world", "hello/world")]
            [TestCase("hello/temp/../temp2/../world", "hello/world")]
            [TestCase("/hello/temp/test/../../world", "/hello/world")]
            [TestCase("/hello/../../../../../../temp", "/temp")]  // Stop collapsing when root is reached
            [TestCase(".", ".")]
            [TestCase("/.", ".")]
            [TestCase("./a", "a")]
            [TestCase("./..", ".")]
            [TestCase("a/./b", "a/b")]
            [TestCase("/a/./b", "/a/b")]
            [TestCase("a/b/.", "a/b")]
            [TestCase("/a/b/.", "/a/b")]
            [TestCase("/./a/b", "/a/b")]
#if !UNIX
            [TestCase("c:/hello/temp/test/../../world", "c:/hello/world")]
            [TestCase("c:/../../../../../../temp", "c:/temp")]
#endif
            public void ShouldCollapseDirectoryPath(string fullPath, string expected)
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(fullPath);

                // When
                string path = NormalizedPath.Collapse(directoryPath);

                // Then
                Assert.AreEqual(expected, path);
            }
            
            [TestCase("/a/b/c/../d/baz.txt", "/a/b/d/baz.txt")]
#if !UNIX
            [TestCase("c:/a/b/c/../d/baz.txt", "c:/a/b/d/baz.txt")]
#endif
            public void ShouldCollapseFilePath(string fullPath, string expected)
            {
                // Given
                FilePath filePath = new FilePath(fullPath);

                // When
                string path = NormalizedPath.Collapse(filePath);

                // Then
                Assert.AreEqual(expected, path);
            }
        }

        public class GetProviderAndPathMethodTests : NormalizedPathTests
        {
            [TestCase("C:/a/b", null, "C:/a/b")]
            [TestCase(@"C:\a\b", null, @"C:\a\b")]
            [TestCase(@"::C::\a\b", null, @"C::\a\b")]
            [TestCase(@"provider::C::\a\b", "provider", @"C::\a\b")]
            [TestCase("/a/b", null, "/a/b")]
            [TestCase("provider::/a/b", "provider", "/a/b")]
            public void ShouldParseProvider(string fullPath, Uri provider, string path)
            {
                // Given, When
                Tuple<Uri, string> result = NormalizedPath.GetProviderAndPath(fullPath);

                // Then
                Assert.AreEqual(provider, result.Item1);
                Assert.AreEqual(path, result.Item2);
            }
        }

        public class EqualsMethodTests : NormalizedPathTests
        {
            [TestCase(true)]
            [TestCase(false)]
            public void SameAssetInstancesIsConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(path.Equals(path));
            }
            
            [TestCase(true)]
            [TestCase(false)]
            public void PathsAreConsideredInequalIfAnyIsNull(bool isCaseSensitive)
            {
                // Given, When
                bool result = new FilePath("test.txt").Equals(null);

                // Then
                Assert.False(result);
            }
            
            [TestCase(true)]
            [TestCase(false)]
            public void SamePathsAreConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(first.Equals(second));
                Assert.True(second.Equals(first));
            }

            [Test]
            public void DifferentPathsAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }

            [Test]
            public void SamePathsButDifferentCasingAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }

            [Test]
            public void SamePathsWithDifferentProvidersAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("foo", "/shaders/basic.vert");
                FilePath second = new FilePath("bar", "/shaders/basic.vert");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }
        }

        public class GetHashCodeMethodTests : NormalizedPathTests
        {
            [Test]
            public void SamePathsGetSameHashCode()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            }

            [Test]
            public void DifferentPathsGetDifferentHashCodes()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
            }

            [Test]
            public void SamePathsButDifferentCasingDoNotGetSameHashCode()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
            }
        }
    }
}
