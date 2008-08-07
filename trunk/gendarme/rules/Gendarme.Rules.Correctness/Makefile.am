correctnessdir = $(pkglibdir)
correctness_SCRIPTS = Gendarme.Rules.Correctness.dll 
correctness_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Correctness.dll Test.Rules.Correctness.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

correctness_sources = \
	ProvideCorrectArgumentsToFormattingMethodsRule.cs \
	AttributeStringLiteralsShouldParseCorrectlyRule.cs

correctness_test_sources = \
	ProvideCorrectArgumentsToFormattingMethodsTest.cs \
	AttributeStringLiteralsShouldParseCorrectlyTest.cs	

correctness_build_sources = $(addprefix $(srcdir)/, $(correctness_sources))
correctness_test_build_sources = $(addprefix $(srcdir)/Test/, $(correctness_test_sources))

Gendarme.Rules.Correctness.dll: $(correctness_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(correctness_build_sources)

Test.Rules.Correctness.dll: $(correctness_test_build_sources) Gendarme.Rules.Correctness.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Correctness.dll -r:Test.Rules.dll -out:$@ $(correctness_test_build_sources)

test: Test.Rules.Correctness.dll

run-test: test
	nunit-console2 Test.Rules.Correctness.dll

run-gendarme: Gendarme.Rules.Correctness.dll
	gendarme Gendarme.Rules.Correctness.dll
