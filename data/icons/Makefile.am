noinst_DATA =					\
	face-sad.png				\
	face-smile.png				\
	favourites-image.png			\
	f-spot-favorite.png			\
	f-spot-other.png			\
	people-image.png			\
	person-image.png			\
	recommended-image.png			\
	secure.png				\
	unknown-cover.png		
	icon-theme-installer

theme=hicolor
themedir=$(datadir)/icons/$(theme)
app_themedir=$(datadir)/$(PACKAGE_NAME)/icons/$(theme)

theme_icons = \
	apps,last-exit-16.png \
	apps,last-exit-22.png \
	apps,last-exit-24.png \
	apps,last-exit.svg
app_icons = \
	actions,show-info-48.svg \
	actions,tag-new-48.svg \
	actions,show-info-32.png \
	actions,tag-new-32.png \
	actions,show-info-22.png \
	actions,tag-new-16.png \
	actions,tag-new-22.png

install_icon_exec = $(top_srcdir)/data/icons/icon-theme-installer \
	-t "$(theme)" \
	-s "$(srcdir)" \
	-d "x$(DESTDIR)" \
	-m "$(mkinstalldirs)" \
	-x "$(INSTALL_DATA)"

MAINTAINERCLEANFILES = Makefile.in
EXTRA_DIST = \
	$(noinst_DATA) \
	icon-theme-installer \
	last-exit-16.png \
	last-exit-22.png \
	last-exit-24.png \
	last-exit.svg \
	show-info-48.svg \
	tag-new-48.svg \
	show-info-32.png \
	tag-new-32.png \
	show-info-22.png \
	tag-new-16.png \
	tag-new-22.png

install-data-local:
	@-$(install_icon_exec) -b "$(themedir)" -i $(theme_icons)
	@-$(install_icon_exec) -b "$(app_themedir)" -i $(app_icons)

uninstall-hook:
	@-$(install_icon_exec) -b "$(themedir)" -u $(theme_icons)
	@-$(install_icon_exec) -b "$(app_themedir)" -i $(app_icons)
