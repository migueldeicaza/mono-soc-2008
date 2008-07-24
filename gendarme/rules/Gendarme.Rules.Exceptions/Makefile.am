exceptionsdir = $(pkglibdir)
exceptions_SCRIPTS = Gendarme.Rules.Exceptions.dll 
exceptions_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Exceptions.dll Test.Rules.Exceptions.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

exceptions_sources = \
	InstantiateArgumentExceptionCorrectlyRule.cs 

exceptions_test_sources = \
	InstantiateArgumentExceptionCorrectlyTest.cs 

exceptions_build_sources = $(addprefix $(srcdir)/, $(exceptions_sources))
exceptions_test_build_sources = $(addprefix $(srcdir)/Test/, $(exceptions_test_sources))

Gendarme.Rules.Exceptions.dll: $(exceptions_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(exceptions_build_sources)

Test.Rules.Exceptions.dll: $(exceptions_test_build_sources) Gendarme.Rules.Exceptions.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Exceptions.dll -r:Test.Rules.dll -out:$@ $(exceptions_test_build_sources)

test: Test.Rules.Exceptions.dll

run-test: test
	nunit-console2 Test.Rules.Exceptions.dll

run-gendarme: Gendarme.Rules.Exceptions.dll
	gendarme Gendarme.Rules.Exceptions.dll
