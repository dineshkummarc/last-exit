GKS_ASSEMBLY = Gnome.Keyring.dll

GKS_SRC = $(srcdir)/Gnome.Keyring/*.cs
DIR_DBUS =  $(top_builddir)/dbus-sharp

if EXTERNAL_DBUS
DBUS_ASSEMBLIES = $(DBUS_LIBS)
else
DBUS_ASSEMBLIES = 				\
	-r:$(DIR_DBUS)/NDesk.DBus.dll		\
	-r:$(DIR_DBUS)/NDesk.DBus.GLib.dll
endif

if !EXTERNAL_GKS
gksdir = $(pkglibdir)
gks_SCRIPTS = \
	$(GKS_ASSEMBLY)

all: $(GKS_ASSEMBLY)

$(GKS_ASSEMBLY): $(GKS_SRC)
	$(MCS) -target:library -out:$@ -unsafe -r:Mono.Posix -d:WITH_DBUS $(DBUS_ASSEMBLIES) $(GKS_SRC)

endif

EXTRA_DIST = $(GKS_SRC)
DISTCLEANFILES = $(GKS_ASSEMBLY) $(GKS_ASSEMBLY).mdb
MAINTAINERCLEANFILES = Makefile.in
