performancedir = $(pkglibdir)
performance_SCRIPTS = Gendarme.Rules.Performance.dll 
performance_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Performance.dll Test.Rules.Performance.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

performance_sources = \
	ConsiderInliningLocalsUsedOnceRule.cs 

performance_test_sources = \
	ConsiderInliningLocalsUsedOnceTest.cs 

performance_build_sources = $(addprefix $(srcdir)/, $(performance_sources))
performance_test_build_sources = $(addprefix $(srcdir)/Test/, $(performance_test_sources))

Gendarme.Rules.Performance.dll: $(performance_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(performance_build_sources)

Test.Rules.Performance.dll: $(performance_test_build_sources) Gendarme.Rules.Performance.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Performance.dll -r:Test.Rules.dll -out:$@ $(performance_test_build_sources)

test: Test.Rules.Performance.dll

run-test: test
	nunit-console2 Test.Rules.Performance.dll

run-gendarme: Gendarme.Rules.Performance.dll
	gendarme Gendarme.Rules.Performance.dll
