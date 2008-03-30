GKS_ASSEMBLY = Gnome.Keyring.dll

GKS_SRC = $(srcdir)/Gnome.Keyring/*.cs

if !EXTERNAL_GKS
gksdir = $(pkglibdir)
gks_SCRIPTS = \
	$(GKS_ASSEMBLY)

all: $(GKS_ASSEMBLY)

$(GKS_ASSEMBLY): $(GKS_SRC)
	$(MCS) -target:library -out:$@ -unsafe -r:Mono.Posix $(GKS_SRC)

endif

EXTRA_DIST = $(GKS_SRC)
DISTCLEANFILES = $(GKS_ASSEMBLY) $(GKS_ASSEMBLY).mdb
MAINTAINERCLEANFILES = Makefile.in
