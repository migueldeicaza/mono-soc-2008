reporterdir = $(pkglibdir)
reporter_SCRIPTS = reporter.exe 
reporter_DATA = 
EXTRA_DIST = 
CLEANFILES = reporter.exe Test.Reporter.dll 
DISTCLEANFILES = Makefile.in TestResult.xml

reporter_sources = \
	Driver.cs \
	Pipeline.cs \
	IAction.cs \
	AddXSLProcessingInstructionAction.cs \
	GenerateMasterIndexAction.cs


reporter_build_sources = $(addprefix $(srcdir)/, $(reporter_sources))


reporter_test_sources = \
	PipelineTest.cs \
	AddXSLProcessingInstructionActionTest.cs\
	GenerateMasterIndexActionTest.cs

reporter_test_build_sources = $(addprefix $(srcdir)/Test/, $(reporter_test_sources))


reporter.exe: $(reporter_build_sources) 
	$(GMCS) -target:exe -r:System.Xml.Linq -out:$@ $(reporter_build_sources)

Test.Reporter.dll: $(reporter_test_build_sources) reporter.exe
	$(GMCS) -target:library -pkg:mono-nunit -r:reporter.exe -r:System.Xml.Linq -out:$@ $(reporter_test_build_sources)

test: Test.Reporter.dll

run-test: test
	nunit-console2 Test.Reporter.dll

run-gendarme: reporter.exe
	gendarme reporter.exe 
