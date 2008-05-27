applyTodir=
applyTo_SCRIPTS = 
applyTo_DATA = 
EXTRA_DIST = 
CLEANFILES = Test.ApplyTo.dll
DISTCLEANFILES = Makefile.in TestResult.xml

applyTo_sources =  \
	ApplyToTest.cs \
	ApplyToAll.cs \
	ApplyToDefault.cs \
	ApplyToNonVisible.cs \
	ApplyToVisible.cs \
	ApplyToMixed.cs \
	FakeRules.cs \
	TestHelpers.cs

applyTo_build_sources = $(addprefix $(srcdir)/, $(applyTo_sources))

all:Test.ApplyTo.dll

Test.ApplyTo.dll: $(applyTo_build_sources) $(applyTo_SCRIPTS)
	$(GMCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -pkg:gendarme-framework \
		-r:/home/nestor/opt/mono/lib/gendarme/gendarme.exe \
		-out:$@ $(applyTo_build_sources)

run-test: Test.ApplyTo.dll
	MONO_PATH=/home/nestor/opt/mono/lib/gendarme nunit-console2 Test.ApplyTo.dll
