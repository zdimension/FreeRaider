-- OPENTOMB SYSTEM CONSOLE NOTIFICATIONS
-- by Lwmte, Apr 2015
-- French Translation by: zdimension and GDFR
-- Translation Version: 1.0.0.0 (May 24th, 2016)

--------------------------------------------------------------------------------
-- Here you have some global system console warnings which will frequently show.
--------------------------------------------------------------------------------

sys_notify[000]     = "Vous devez entrer l'ID de l'entité !";
sys_notify[001]     = "Paramètres incorrects, devraient être %s.";
sys_notify[002]     = "Nombre de paramètres incorrect, devrait être %s.";
sys_notify[003]     = "Impossible de trouver l'entité avec l'ID #%d !";
sys_notify[004]     = "Index d'option incorrect, %d est le maximum.";
sys_notify[005]     = "Aucune entité ou personnage avec l'ID #%d !";
sys_notify[006]     = "ID de pièce invalide : #%d!";
sys_notify[007]     = "Il n'y a pas de modèles avec l'ID #%d.";
sys_notify[008]     = "Numéro d'action incorrect.";
sys_notify[009]     = "Impossible de créer la police. Maximum peut-être atteint ? (%d / %d)";
sys_notify[010]     = "Impossible de créer le style. Maximum peut-être atteint ? (%d / %d)";
sys_notify[011]     = "La police avec l'ID donné n'existe pas ou n'a pas pu être supprimée !";
sys_notify[012]     = "Le style avec l'ID donné n'existe pas ou n'a pas pu être supprimé !";
sys_notify[013]     = "Impossible de trouver le modèle squelettique avec l'ID #%d !";
sys_notify[014]     = "Numéro d'animation incorrect: #%d !";
sys_notify[015]     = "Numéro de dispatch d'animation inccorect: #%d !";
sys_notify[016]     = "Numéro de frame incorrect: #%d !";
sys_notify[017]     = "ID de flux incorrect. Devrait être supérieur ou égal à 0.";
sys_notify[018]     = "ID de son incorrect. Devrait être dans l'intervalle 0..%d.";
sys_notify[019]     = "Erreur Audio_Send: aucun canal libre";
sys_notify[020]     = "Erreur Audio_Send: aucun échantillon trouvé.";
sys_notify[021]     = "Audio_Send: échantillon ignoré - veuillez réessayer !";
sys_notify[022]     = "Audio_Kill: l'échantillon %d n'est pas en lecture.";
sys_notify[023]     = "L'état du bit de flipmap %d est FALSE - aucune pièce n'a été échangée !";
sys_notify[024]     = "Fichier non trouvé - \"%s\"";
sys_notify[025]     = "Warning: l'image %s n'est pas en truecolor - non supportée !";
sys_notify[026]     = "SDL n'a pas pu charger %s : %s";
sys_notify[027]     = "Offset d'image incorrect";
sys_notify[028]     = "Error: impossible d'ouvrir le fichier !";
sys_notify[029]     = "Error: format de fichier incorrect !";
sys_notify[030]     = "Nombre de lignes de console incorrect !";
sys_notify[031]     = "La flipmap avec l'index %d n'existe pas !";
sys_notify[032]     = "La configuration de chevelure avec l'index %d n'existe pas !";
sys_notify[033]     = "Impossible de créer la chevelure pour le personnage %d !";
sys_notify[034]     = "Pas de chevelure pour le personnage %d - rien à supprimer.";
sys_notify[035]     = "La configuration de ragdoll avec l'index %d est corrompue ou n'existe pas !";
sys_notify[036]     = "Impossible de créer le ragdoll pour l'entité %d !";
sys_notify[037]     = "Impossible de supprimer le ragdoll de l'entité %d - peut-être pas de ragdoll ?";
sys_notify[038]     = "Load_Wad: nombre de pistes hors limites - max. %d.";
sys_notify[039]     = "Load_Wad: impossible de lire à la position %X.";
sys_notify[040]     = "StreamPlay: CANCEL, index de piste hors limites (%d).";
sys_notify[041]     = "StreamPlay: CANCEL, piste déjà en lecture (%d).";
sys_notify[042]     = "StreamPlay: CANCEL, index de piste incorrect ou script cassé (%d).";
sys_notify[043]     = "StreamPlay: CANCEL, aucun flux libre.";
sys_notify[044]     = "StreamPlay: CANCEL, erreur lors du chargement du flux.";
sys_notify[045]     = "StreamPlay: CANCEL, erreur lors de la lecture du flux.";
sys_notify[046]     = "Information de secteur incorrecte !";
sys_notify[047]     = "Numéro de membre incorrect : %d";
sys_notify[048]     = "Warning: l'image %s n'a pas pu être chargée, le statut est %d.";
sys_notify[049]     = "ID de modèle incorrect : %d";
sys_notify[050]     = "Impossible de trouver l'entité %d ou le numéro de corps est supérieur à %d !";
sys_notify[051]     = "Impossible d'appliquer la force à l'entité %d - pas d'entité ou de corps, ou l'entité n'est pas dynamique !";
sys_notify[052]     = "Le numéro d'axe %d est incorrect !";


sys_notify[1000]    = "Piste ouverte (%s): canaux = %d, taux d'échantillonnage = %d";
sys_notify[1001]    = "Lecture du fichier : \"%s\"";
sys_notify[1002]    = "Donne l'item %i x%i à l'entité %x";
sys_notify[1003]    = "L'ID de niveau actuel devient %d";
sys_notify[1004]    = "Le jeu actuel devient %d";
sys_notify[1005]    = "Version du moteur Tomb Raider: %d, carte : \"%s\"";
sys_notify[1006]    = "Nombre de pièces : %d";
sys_notify[1007]    = "Nombre de textures : %d";
sys_notify[1008]    = "Espacement des lignes de console : %f";
sys_notify[1009]    = "Nombre de lignes de console: %d";
sys_notify[1010]    = "DÉCLENCHEUR : chronomètre - %d, masque - %02X";
sys_notify[1011]    = "Activer l'objet %d par %d";
sys_notify[1012]    = "Image de fondu chargée : %s";
sys_notify[1013]    = "Tableau de déclencheurs correctement nettoyé !";
sys_notify[1014]    = "Tableau de fonctions d'entités correctement nettoyé !";
sys_notify[1015]    = "Niveau PC chargé (chargeur VT utilisé).";
sys_notify[1016]    = "Niveau PSX chargé.";
sys_notify[1017]    = "Niveau Dreamcast chargé.";
sys_notify[1018]    = "Niveau OpenTomb chargé.";
sys_notify[1019]    = "Lecture du WAD (%s): position %X, taille %X.";
sys_notify[1020]    = "Moteur initialisé !";
sys_notify[1021]    = "Index de pile Lua actuel : %d";
sys_notify[1022]    = "Chargement de la carte : %s";

sys_notify[1023]    = "Commandes disponibles :";
sys_notify[1024]    = "  help - afficher l'aide";
sys_notify[1025]    = "  cls - vider la console";
sys_notify[1026]    = "  setgamef(jeu, niveau) - charger le jeu et le niveau spécifié";
sys_notify[1027]    = "  loadMap(\"fichier\") - charger un fichier de niveau \"fichier\"";
sys_notify[1028]    = "  save, load - sauvegarder et charger des parties \"file_name\"";
sys_notify[1029]    = "  exit - quitter le jeu";
sys_notify[1030]    = "  show_fps - basculer le compteur de FPS";
sys_notify[1031]    = "  free_look - basculer le mode de caméra";
sys_notify[1032]    = "  mlook - basculer la vue à la souris";
sys_notify[1033]    = "  noclip - mode DOZY";
sys_notify[1034]    = "  cam_distance - distance de la caméra";
sys_notify[1035]    = "  time_scale - basculer la vitesse du temps (ralenti)";
sys_notify[1036]    = "  r_coll, r_portals, r_frustums, r_room_boxes, r_boxes, r_normals, r_skip_room - render modes";
sys_notify[1037]    = "Attention à la CaSsE des commandes !";